using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Client.Models;
using CsvHelper;

namespace CHC.Consent.DataImporter.Features.ExportData
{
    internal class ValueOutputFormatter : IDefinitionVisitor<IdentifierDefinition>
    {
        private ILookup<string, string[]> FieldNames { get; }
        private IEnumerable<IdentifierDefinition> IdentifierDefinitions { get; }

        /// <inheritdoc />
        public ValueOutputFormatter(IList<IdentifierDefinition> definitions, IEnumerable<string[]> fieldNames)
        {
            FieldNames = fieldNames.ToLookup(_ => _.First(), _ => _.Skip(1).ToArray());
            var topLevelFieldNames = FieldNames.Select(_ => _.Key).ToArray();

            IdentifierDefinitions = GetDefinitionsInOutputOrder(definitions, topLevelFieldNames);
            this.VisitAll(IdentifierDefinitions);
        }

        private static IEnumerable<IdentifierDefinition> GetDefinitionsInOutputOrder(
            IEnumerable<IdentifierDefinition> definitions, 
            IEnumerable<string> topLevelFieldNames)
        {
            return topLevelFieldNames.Select(definitions.GetDefinition).ToArray();
        }

        private delegate void Writer(IIdentifierValueDto dto, IWriterRow writer);

        private IDictionary<string, Writer> Writers { get; } = new Dictionary<string, Writer>();

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, CompositeDefinitionType type)
        {
            var subWriters = new ValueOutputFormatter(
                type.Identifiers.Cast<IdentifierDefinition>().ToArray(),
                FieldNames[definition.SystemName].ToArray()
            );

            Writers[definition.SystemName] = subWriters.WriteCompositeValue;
        }

        private void WriteCompositeValue(IIdentifierValueDto dto, IWriterRow writer)
        {
            var compositeDto = (IdentifierValueDtoIIdentifierValueDto) dto;
            var subValues = compositeDto?.Value ?? Enumerable.Empty<IIdentifierValueDto>();

            Write(subValues, writer);
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, DateDefinitionType type)
        {
            Writers[definition.SystemName] = WriteDate;
        }

        private static Writer WriteDate { get; } =
            (dto, writer) => writer.WriteField(((IdentifierValueDtoDateTime) dto)?.Value?.ToString("yyyy-MM-dd"));

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, EnumDefinitionType type)
        {
            Writers[definition.SystemName] = WriteEnum;
        }

        private static Writer WriteEnum { get; } =
            (dto, writer) => writer.WriteField(((IdentifierValueDtoString) dto)?.Value);


        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, IntegerDefinitionType type)
        {
            Writers[definition.SystemName] = WriteInteger;
        }

        private static Writer WriteInteger { get; } = 
            (dto, writer) => writer.WriteField(((IdentifierValueDtoInt64) dto)?.Value);

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, StringDefinitionType type)
        {
            Writers[definition.SystemName] = WriteString;
        }

        private static Writer WriteString { get; } =
            (dto, writer) => writer.WriteField(((IdentifierValueDtoString) dto)?.Value);

        public void Write(
            IEnumerable<IIdentifierValueDto> identifiers,
            IWriterRow destination)
        {
            //TODO: Handle multiple values
            var valuesByName = identifiers.ToLookup(_ => _.Name).ToDictionary(_ => _.Key, _ => _.First());
            foreach (var identifierDefinition in IdentifierDefinitions)
            {
                valuesByName.TryGetValue(identifierDefinition.SystemName, out var identifier);
                Writers[identifierDefinition.SystemName](identifier, destination);
            }
        }
    }
}