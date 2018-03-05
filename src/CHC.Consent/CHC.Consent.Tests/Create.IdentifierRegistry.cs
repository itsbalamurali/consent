using System;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore;

namespace CHC.Consent.Tests
{
    public static partial class Create 
    {
        public class IdentifierRegistryBuilder : Builder<ITypeRegistry<IPersonIdentifier>, IdentifierRegistryBuilder>
        {
            private Type[] identifierTypes = Array.Empty<Type>();

            public IdentifierRegistryBuilder WithIdentifier<T>() where T : IPersonIdentifier
            {
                return Copy(change: @new => @new.identifierTypes = @new.identifierTypes.Append(typeof(T)).ToArray());
            }

            public IdentifierRegistryBuilder WithIdentifiers<T1, T2>() 
                where T1 : IPersonIdentifier 
                where T2 : IPersonIdentifier
                => WithIdentifier<T1>().WithIdentifier<T2>();

            /// <inheritdoc />
            public override ITypeRegistry<IPersonIdentifier> Build()
            {
                var registry = new TypeRegistry<IPersonIdentifier,IdentifierAttribute>();
                foreach (var identifierType in identifierTypes)
                {
                    registry.Add(identifierType, IdentifierAttribute.GetAttribute(identifierType));
                }

                return registry;
            }
        }

        public static IdentifierRegistryBuilder IdentifierRegistry => new IdentifierRegistryBuilder();
    }
}