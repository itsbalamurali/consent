using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Client.Models;

namespace CHC.Consent.DataTool.Features.ExportData
{
    public class FieldNameList : IDefinitionVisitor<IdentifierDefinition>, IEnumerable<string>
    {
        public static FieldNameList CreateFromDefinitions(IEnumerable<IdentifierDefinition> definitions)
        {
            return new FieldNameList(definitions);
        }

        private const string Separator = "::";

        private static string Join(IDefinition definition, string compositeName) =>
            Join(definition.SystemName, compositeName);

        private static string Join(params string[] parts) => string.Join(Separator, parts);

        /// <inheritdoc />
        private FieldNameList(IEnumerable<IdentifierDefinition> definitions)
        {
            this.VisitAll(definitions);
        }

        private ICollection<string> Names { get;  } = new HashSet<string>();

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, CompositeDefinitionType type)
        {
            var compositeNames = CreateFromDefinitions(type.Identifiers.Cast<IdentifierDefinition>());
            foreach (var compositeName in compositeNames)
            {
                Names.Add(Join(definition, compositeName));
            }
        }

        private void AddName(IdentifierDefinition definition) => Names.Add($"{definition.SystemName}");

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, DateDefinitionType type) => AddName(definition);

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, EnumDefinitionType type) => AddName(definition);
        
        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, IntegerDefinitionType type) => AddName(definition);
        
        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, StringDefinitionType type) => AddName(definition);
        
        /// <inheritdoc />
        public IEnumerator<string> GetEnumerator() => Names.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Names).GetEnumerator();

        public IEnumerable<string> Except(IEnumerable<string> fullFieldNames)
        {
            return fullFieldNames.Except(Names);
        }

        public static IEnumerable<string> FullFieldNames(string[][] strings)
        {
            return strings.Select(Join);
        }

        public static string[][] Split(IEnumerable<string> fieldNames)
        {
            return fieldNames.Select(_ => _.Split(Separator)).ToArray();
        }
    }
}