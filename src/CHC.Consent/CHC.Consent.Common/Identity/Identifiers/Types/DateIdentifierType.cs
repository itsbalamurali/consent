using System;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class DateIdentifierType : IIdentifierType
    {
        /// <inheritdoc />
        public virtual void Accept(IIdentifierDefinitionVisitor visitor, IdentifierDefinition definition)
        {
            visitor.Visit(definition, this);
        }

        /// <inheritdoc />
        public string SystemName => "date";
    }
}