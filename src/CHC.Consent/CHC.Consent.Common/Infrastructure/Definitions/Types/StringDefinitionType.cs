namespace CHC.Consent.Common.Infrastructure.Definitions.Types
{
    public class StringDefinitionType : IDefinitionType 
    {
        /// <inheritdoc />
        public void Accept(IDefinitionVisitor visitor, IDefinition definition) =>
            visitor.Visit(definition, this);

        /// <inheritdoc />
        public string SystemName => "string";
    }
}