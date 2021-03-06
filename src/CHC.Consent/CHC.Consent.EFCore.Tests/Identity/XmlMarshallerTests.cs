﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Definitions;
using CHC.Consent.Common.Infrastructure.Definitions.Types;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.Testing.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Xunit;
using Random = CHC.Consent.Testing.Utils.Random;

namespace CHC.Consent.EFCore.Tests.Identity
{
    public class XmlMarshallerTests
    {
        public static TheoryData<IDefinitionType, object, string> TestData =>
            new TheoryData<IDefinitionType, object, string>
            {
                {new StringDefinitionType(), "Some Value", "Some Value"},
                {new EnumDefinitionType("One", "Two"), "One", "One"},
                {new DateDefinitionType(), 15.July(2041), "2041-07-15T00:00:00"},
                {new IntegerDefinitionType(), 263762L, "263762"}
            };

        private static IIdentifierXmlMarhsaller<PersonIdentifier, IdentifierDefinition> CreateMarshallerFor(IdentifierDefinition identifierDefinition)
        {
            var marshallerCreator = new IdentifierXmlMarshallerCreator<PersonIdentifier, IdentifierDefinition>();
            identifierDefinition.Accept(marshallerCreator);
            return marshallerCreator.Marshallers.Values.Single();
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void StringMarshaller_CorrectlyMarshalsValue(
            IDefinitionType definitionType, object value, string expectedText)
        {
            
            var identifier = new PersonIdentifier(
                new SimpleIdentifierValue(value),
                new IdentifierDefinition("Identifier", definitionType));

            var marshaller = CreateMarshallerFor(identifier.Definition);

            var marshalledValue = marshaller.MarshallToXml(identifier);

            Assert.Equal(
                new XElement("identifier", expectedText).ToString(SaveOptions.DisableFormatting),
                marshalledValue.ToString(SaveOptions.DisableFormatting));
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void StringMarshaller_CorrectlyMarshalsFromXml(
            IDefinitionType definitionType, object expected, string innerXml)
        {
            var definition = new IdentifierDefinition("Identifier", definitionType);

            var marshaller = CreateMarshallerFor(definition);

            var identifier = marshaller.MarshallFromXml(new XElement("identifier", innerXml));

            Assert.Equal(expected,identifier.Value.Value
            );
        }

        [Fact]
        public void CompositeMarshaller_MarshalsToXmlCorrectly()
        {
            var (compositeIdentifierDefinition, stringIdentifierDefinition, dateIdentifierDefinition) = CompositeIdentifierDefinition();

            var identifier = new PersonIdentifier(
                new CompositeIdentifierValue<PersonIdentifier>(
                    new []
                    {
                        new PersonIdentifier(new SimpleIdentifierValue("A Name"), stringIdentifierDefinition),
                        new PersonIdentifier(new SimpleIdentifierValue(17.April(1872)), dateIdentifierDefinition)
                    }), 
                compositeIdentifierDefinition);

            var marshaller = new CompositeIdentifierXmlMarshaller<PersonIdentifier, IdentifierDefinition>(compositeIdentifierDefinition);

            var xml = marshaller.MarshallToXml(identifier);

            Assert.Equal(
                "<composite><string>A Name</string><date>1872-04-17T00:00:00</date></composite>",
                xml.ToString(SaveOptions.DisableFormatting));
        }
        
        
        [Fact]
        public void CompositeMarshaller_MarshalsFromXmlCorrectly()
        {
            var (compositeIdentifierDefinition, stringIdentifierDefinition, dateIdentifierDefinition) = CompositeIdentifierDefinition();

            var marshaller = new CompositeIdentifierXmlMarshaller<PersonIdentifier, IdentifierDefinition>(compositeIdentifierDefinition);

            var identifier = marshaller.MarshallFromXml(XElement.Parse("<composite><string>A Name</string><date>1872-04-17T00:00:00</date></composite>"));

            identifier.Value.As<CompositeIdentifierValue<PersonIdentifier>>().Identifiers.Should()
                .Contain(new PersonIdentifier(new SimpleIdentifierValue("A Name"), stringIdentifierDefinition))
                .And
                .Contain(new PersonIdentifier(new SimpleIdentifierValue(17.April(1872)), dateIdentifierDefinition))
                .And
                .HaveCount(2);
        }


        [Fact]
        public void CompositeMarshaller_CreatesMarshallersForInnerDefinitions()
        {
            var (composite, _, _) = CompositeIdentifierDefinition();
            
            var marshaller = new CompositeIdentifierXmlMarshaller<PersonIdentifier, IdentifierDefinition>(composite);

            Assert.Contains("string", marshaller.Marshallers.Marshallers);
            Assert.Contains("date", marshaller.Marshallers.Marshallers);
        }

        private static (
            IdentifierDefinition compositeIdentifierDefinition,
            IdentifierDefinition stringIdentifierDefinition, 
            IdentifierDefinition dateIdentifierDefinition
            ) CompositeIdentifierDefinition()
        {
            var stringIdentifierDefinition = new IdentifierDefinition("String", new StringDefinitionType());
            var dateIdentifierDefinition = new IdentifierDefinition("Date", new DateDefinitionType());
            var compositeIdentifierDefinition = new IdentifierDefinition(
                "Composite",
                new CompositeDefinitionType(
                    stringIdentifierDefinition,
                    dateIdentifierDefinition));
            return (compositeIdentifierDefinition, stringIdentifierDefinition, dateIdentifierDefinition);
        }
    }
}