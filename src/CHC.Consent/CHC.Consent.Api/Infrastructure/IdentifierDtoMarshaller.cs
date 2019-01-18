using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Api.Infrastructure
{
    public class IdentifierDtoMarshaller<TIdentifier, TDefinition>
        where TDefinition : DefinitionBase
        where TIdentifier : IIdentifier<TDefinition>
    {
        private IDictionary<string, IMarshaller> Marshallers { get; }

        public IdentifierDtoMarshaller(DefinitionRegistry registry, CreateIdentifier createIdentifier)
        {
            Marshallers = registry.Accept(new IdentifierIdentifierValueDtoMarshaller(createIdentifier)).Marshallers;
        }

        public IIdentifierValueDto[] MarshallToDtos(IEnumerable<TIdentifier> identifiers)
        {
            return identifiers.Select(MarshallToDto).ToArray();
        }

        private IIdentifierValueDto MarshallToDto(TIdentifier identifier)
        {
            return Marshallers[identifier.Definition.SystemName].MarshallToDto(identifier);
        }

        public TIdentifier[] ConvertToIdentifiers(IEnumerable<IIdentifierValueDto> dtos)
        {
            return dtos.Select(MarshallToIdentifier).ToArray();
        }

        private TIdentifier MarshallToIdentifier(IIdentifierValueDto dto)
        {
            return Marshallers[dto.DefinitionSystemName].MarshallToIdentifier(dto);
        }

        public delegate TIdentifier CreateIdentifier(TDefinition definition, IIdentifierValue value);

        private class IdentifierIdentifierValueDtoMarshaller : IDefinitionVisitor
        {
            public IDictionary<string, IMarshaller> Marshallers { get; } = new Dictionary<string, IMarshaller>();
            private CreateIdentifier IdentifierCreator { get; }

            /// <inheritdoc />
            public IdentifierIdentifierValueDtoMarshaller(CreateIdentifier identifierCreator)
            {
                IdentifierCreator = identifierCreator;
            }


            private void UseSimpleMarshaller<T>(IDefinition definition)
                => UseMarshaller(definition, new SimpleMarshaller<T>((TDefinition) definition, IdentifierCreator));

            private void UseMarshaller(
                IDefinition definition,
                IMarshaller marshaller)
            {
                Marshallers[definition.SystemName] = marshaller;
            }

            /// <inheritdoc />
            public void Visit(IDefinition definition, DateIdentifierType type)
            {
                UseSimpleMarshaller<DateTime>(definition);
            }

            /// <inheritdoc />
            public void Visit(IDefinition definition, EnumIdentifierType type)
            {
                UseSimpleMarshaller<string>(definition);
            }

            /// <inheritdoc />
            public void Visit(IDefinition definition, CompositeIdentifierType type)
            {
                UseMarshaller(definition, new CompositeMarshaller(type, (TDefinition) definition, IdentifierCreator));
            }

            /// <inheritdoc />
            public void Visit(IDefinition definition, IntegerIdentifierType type)
            {
                UseSimpleMarshaller<long>(definition);
            }

            /// <inheritdoc />
            public void Visit(IDefinition definition, StringIdentifierType type)
            {
                UseSimpleMarshaller<string>(definition);
            }
        }

        private interface IMarshaller
        {
            IIdentifierValueDto MarshallToDto(TIdentifier identifier);
            TIdentifier MarshallToIdentifier(IIdentifierValueDto dto);
        }

        private class SimpleMarshaller<TValue> : IMarshaller
        {
            private TDefinition Definition { get; }
            private CreateIdentifier CreateIdentifier { get; }

            /// <inheritdoc />
            public SimpleMarshaller(TDefinition definition, CreateIdentifier createIdentifier)
            {
                Definition = definition;
                CreateIdentifier = createIdentifier;
            }

            /// <inheritdoc />
            public IIdentifierValueDto MarshallToDto(TIdentifier identifier)
            {
                return
                    new IdentifierValueDto<TValue>(
                        identifier.Definition.SystemName,
                        (TValue) identifier.Value.Value
                    );
            }

            /// <inheritdoc />
            public TIdentifier MarshallToIdentifier(IIdentifierValueDto dto)
            {
                return CreateIdentifier(Definition, new SimpleIdentifierValue(dto.Value));
            }
        }

        private class CompositeMarshaller : IMarshaller
        {
            public TDefinition Definition { get; }
            public CreateIdentifier CreateIdentifier { get; }
            private readonly IdentifierDtoMarshaller<TIdentifier, TDefinition> identifierDtoMarshaller;

            /// <inheritdoc />
            public CompositeMarshaller(
                CompositeIdentifierType type, TDefinition definition, CreateIdentifier createIdentifier)
            {
                Definition = definition;
                CreateIdentifier = createIdentifier;
                identifierDtoMarshaller = new IdentifierDtoMarshaller<TIdentifier, TDefinition>(type.Identifiers, createIdentifier);
            }

            public IIdentifierValueDto MarshallToDto(TIdentifier identifier)
            {
                var values = ((CompositeIdentifierValue<TIdentifier>) identifier.Value).Identifiers;
                return
                    new IdentifierValueDto<IIdentifierValueDto[]>(
                        identifier.Definition.SystemName,
                        identifierDtoMarshaller.MarshallToDtos(values)
                    );
            }

            /// <inheritdoc />
            public TIdentifier MarshallToIdentifier(IIdentifierValueDto dto)
            {
                var value = (IIdentifierValueDto[]) dto.Value;
                var identifiers = identifierDtoMarshaller.ConvertToIdentifiers(value);

                return CreateIdentifier(Definition, new CompositeIdentifierValue<TIdentifier>(identifiers));
            }
        }
    }
}