using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Definitions;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.EFCore.Security;
using CHC.Consent.Testing.Utils;
using FakeItEasy.Sdk;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Formatting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using Xunit.Abstractions;
using Random = CHC.Consent.Testing.Utils.Random;

namespace CHC.Consent.EFCore.Tests
{
    public class IdentityRepositoryTests : DbTests
    {
        private readonly string personOneNhsNumber = Random.String();
        private readonly string personTwoNhsNumber = Random.String();
        private readonly PersonEntity personOne;
        private readonly PersonEntity personTwo;
        private readonly IdentityRepository repository;
        private readonly IdentifierDefinition testIdentifierDefinition;
        private readonly AuthorityEntity defaultAuthority;

        private PersonIdentity FindPersonByNhsNumber(string nhsNumber)
        {
            return repository.FindPersonBy(Identifiers.NhsNumber(nhsNumber));
        }

        private PersonIdentifierEntity NhsNumberEntity(PersonEntity personEntity, string nhsNumber, AuthorityEntity authority=null)
        {
            return IdentifierEntity(personEntity, Identifiers.Definitions.NhsNumber, nhsNumber, authority);
        }

        private PersonIdentifierEntity IdentifierEntity(
            PersonEntity personEntity, IdentifierDefinition identifierDefinition, string value, AuthorityEntity authority)
        {
            return new PersonIdentifierEntity
            {
                Person = personEntity,
                TypeName = identifierDefinition.SystemName,
                Value = MarshallValue(identifierDefinition, value),
                ValueType = identifierDefinition.Type.SystemName,
                Authority = authority ?? defaultAuthority
            };
        }

        private static string MarshallValue(IdentifierDefinition identifierDefinition, string value)
        {
            return new IdentifierXmlElementMarshaller<PersonIdentifier, IdentifierDefinition>(identifierDefinition)
                .MarshallToXml(Identifiers.PersonIdentifier(value, identifierDefinition))
                .ToString(SaveOptions.DisableFormatting);
        }

        /// <inheritdoc />
        public IdentityRepositoryTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
            Formatter.AddFormatter(PersonIdentifierFormatter.Instance);
            Formatter.AddFormatter(DictionaryIdentifierFormatter.Instance);
            
            personOne = new PersonEntity();
            personTwo = new PersonEntity();
            defaultAuthority = new AuthorityEntity("Default " + Random.String(), 1000, Random.String());
            
            Context.AddRange(personOne, personTwo);
            Context.Add(defaultAuthority);
            
            testIdentifierDefinition = Identifiers.Definitions.String("Test");
            Context.AddRange(
                NhsNumberEntity(personOne, personOneNhsNumber), 
                NhsNumberEntity(personTwo, personTwoNhsNumber),
                IdentifierEntity(personOne, testIdentifierDefinition, personTwoNhsNumber, defaultAuthority)
                );
            Context.SaveChanges();
            
            Assert.NotNull(readContext.People.Find(personOne.Id));
            Assert.NotNull(readContext.People.Find(personTwo.Id));
            Assert.Equal(2, readContext.Set<PersonIdentifierEntity>().Count(_ => _.Person == personOne));
            Assert.Single(readContext.Set<PersonIdentifierEntity>().Where(_ => _.Person == personTwo));

            repository = CreateRepository(CreateNewContextInSameTransaction());
        }

        private IdentityRepository CreateRepository(ConsentContext context)
        {
            var registry = new IdentifierDefinitionRegistry(Identifiers.Definitions.KnownIdentifiers)
            {
                testIdentifierDefinition
            };

            return new IdentityRepository(registry, context);
        }

        [Fact]
        public void CanFindAPersonOneByNhsNumber()
        {
            Assert.Equal(personOne, FindPersonByNhsNumber(personOneNhsNumber));
        }

        [Fact]
        public void CanFindAPersonTwoByNhsNumber() =>
            Assert.Equal(personTwo, FindPersonByNhsNumber(personTwoNhsNumber));

        [Fact]
        public void ReturnsNullForUnknownPerson() => Assert.Null(FindPersonByNhsNumber("UNKNOWN"));


        [Fact]
        public void UpdatesIdentifiersCorrectly()
        {
            var localRepository = CreateRepository(updateContext);
            var newTestValue = Random.String();
            localRepository.UpdatePerson(
                new PersonIdentity(personOne.Id),
                new[]
                {
                    Identifiers.PersonIdentifier(newTestValue, testIdentifierDefinition),
                    Identifiers.NhsNumber(personOneNhsNumber)
                    
                },
                defaultAuthority.ToAuthority());
            updateContext.SaveChanges();
            

            var identifierEntities = readContext.PersonIdentifiers.Where(_ => _.Person.Id == personOne.Id).ToArray();

            identifierEntities.Should()
                .ContainSingleIdentifierValue(testIdentifierDefinition, personTwoNhsNumber, deleted: true)
                .And
                .ContainSingleIdentifierValue(testIdentifierDefinition, newTestValue)
                .And
                .ContainSingleIdentifierValue(Identifiers.Definitions.NhsNumber, personOneNhsNumber);

        }

        [Fact]
        public void CorrectlyLoadsCurrentIdentifiers()
        {
            var localRepository = CreateRepository(updateContext);
            var newTestValue = Random.String();
            localRepository.UpdatePerson(
                new PersonIdentity(personOne.Id),
                new[]
                {
                    Identifiers.PersonIdentifier(newTestValue, testIdentifierDefinition)
                },
                defaultAuthority.ToAuthority());
            updateContext.SaveChanges();

            var personIdentifiers = CreateRepository(readContext).GetPersonIdentifiers(personOne.Id).ToArray();

            personIdentifiers.Should().HaveCount(2, "because there are two active identifiers");


            personIdentifiers.Should().ContainSingle(
                _ => Equals(_.Value.Value, personOneNhsNumber) && _.Definition == Identifiers.Definitions.NhsNumber);


            personIdentifiers.Should().ContainSingle(
                _ => Equals(_.Value.Value, newTestValue) && _.Definition == testIdentifierDefinition);
        }

        [Fact]
        public void CreatesCorrectIdentifiers()
        {
            var testValue = Random.String();
            var nhsNumber = Random.String();
            var newPersonIdentity = CreateRepository(createContext).CreatePerson(
                new[]
                {
                    Identifiers.NhsNumber(nhsNumber),
                    Identifiers.PersonIdentifier(testValue, testIdentifierDefinition)
                },
                defaultAuthority.ToAuthority()
            );
            createContext.SaveChanges();

            var newPerson = readContext.People.Find(newPersonIdentity.Id);
            newPerson.Should().NotBeNull();

            var createdIdentifiers = readContext.PersonIdentifiers.Where(_ => _.Person == newPerson).ToArray();

            createdIdentifiers.Should().HaveCount(2);

            createdIdentifiers.Should().NotContain(_ => _.Deleted != null);
            createdIdentifiers.Should()
                .ContainSingleIdentifierValue(testIdentifierDefinition, testValue)
                .And
                .ContainSingleIdentifierValue(Identifiers.Definitions.NhsNumber, nhsNumber);

        }

        [Fact]
        public void AuthorityPriorityIsRespected()
        {
            var higherAuthority =
                createContext.Add(new AuthorityEntity("Higher", defaultAuthority.Priority - 1, "higher")).Entity
                    .ToAuthority();
            
            var thePerson = CreateRepository(createContext).CreatePerson(
                new[]
                {
                    Identifiers.NhsNumber("1234567890"),
                    Identifiers.Address("22 Medway Street", postcode: "Will Be Replaced")
                },
                defaultAuthority.ToAuthority());
            createContext.SaveChanges();

            var updatedAddress = Identifiers.Address("21 Spine Road", postcode: "Has Replaced");
            CreateRepository(updateContext).UpdatePerson(
                (PersonIdentity)thePerson.Id,
                new [] { updatedAddress },
                higherAuthority);
            updateContext.SaveChanges();

            var newContext = CreateNewContextInSameTransaction();
            CreateRepository(newContext).UpdatePerson(
                (PersonIdentity)thePerson.Id,
                new [] { Identifiers.Address(postcode:"Should Ignore This")},
                defaultAuthority.ToAuthority()
                );
            newContext.SaveChanges();

            var identifiers = readContext.PersonIdentifiers.Where(_ => _.Person == thePerson && _.Deleted == null).ToArray();

            identifiers.Should().ContainSingleIdentifierValue(Identifiers.Definitions.NhsNumber, "1234567890")
                .And
                .ContainSingleIdentifierValue(updatedAddress);
        }

        [Fact]
        public void CanPersistCompositeIdentifiers()
        {
            var name = Identifiers.Name("Francis", "Drake");
            var nhsNumber = Identifiers.NhsNumber(Random.String());
            var person = CreateRepository(createContext).CreatePerson(new[]{ name, nhsNumber }, defaultAuthority.ToAuthority());
            createContext.SaveChanges();

            var personIdentifiers = CreateRepository(readContext).GetPersonIdentifiers(person);

            personIdentifiers.Should().Contain(name)
                .And.Contain(nhsNumber)
                .And.HaveCount(2)
                .And.OnlyHaveUniqueItems();
        }

        [Fact]
        public void BringsBackCorrectIdentifiers()
        {
            var personIdentity = new PersonIdentity(personOne.Id);
            var foundDetails = CreateRepository(readContext)
                .GetPeopleWithIdentifiers(
                    new[] {personIdentity},
                    new[] {testIdentifierDefinition.SystemName},
                    null
                );

            foundDetails.Should().ContainKey(personIdentity)
                .WhichValue.Should()
                .OnlyContain(identifier => identifier.Definition == testIdentifierDefinition);
        }

        [Fact]
        public void CanSearchForSimpleIdentifiers()
        {
            var nhsNumber = Random.String();
            var nhsNumberIdentifier = Identifiers.NhsNumber(nhsNumber);
            
            var createRepository = CreateRepository(createContext);
            var person = createRepository.CreatePerson(new[]{ nhsNumberIdentifier }, defaultAuthority.ToAuthority());
            createContext.SaveChanges();


            var found = People(readContext)
                    .Search(
                        readContext,
                        new HasIdentifiersCriteria(
                            new IdentifierSearch
                                {IdentifierName = nhsNumberIdentifier.Definition.SystemName, Value = nhsNumber}))
                    .ToArray()
                ;

            found.Should().OnlyContain(_ => _.Id == person.Id);
        }

        private IIncludableQueryable<PersonEntity, AccessControlList> People(ConsentContext context)
        {
                return context.People.AsNoTracking()
                    .Include(_ => _.Identifiers)
                    .Include(_ => _.ACL);
        }

        [Fact]
        public void CanSearchForMultipleCriteria()
        {
            var found =
                People(readContext).Search(
                        readContext,
                        new HasIdentifiersCriteria(
                            new IdentifierSearch
                                {IdentifierName = testIdentifierDefinition.SystemName, Value = personTwoNhsNumber},
                            new IdentifierSearch
                            {
                                IdentifierName = Identifiers.Definitions.NhsNumber.SystemName,
                                Value = personOneNhsNumber
                            }
                        )
                    )
                    .ToArray();

            found.Should().BeEquivalentTo(personOne);

        }
    }

    public class PersonIdentifierFormatter : IValueFormatter
    {
        /// <inheritdoc />
        public bool CanHandle(object value) => value is PersonIdentifier;

        /// <inheritdoc />
        public string Format(object value, FormattingContext context, FormatChild formatChild)
        {            
            var personIdentifier = (PersonIdentifier)value;
            return $"{personIdentifier.Definition.SystemName}: {formatChild("Value", personIdentifier.Value.Value)}";
        }
        
        public static IValueFormatter Instance { get; } = new PersonIdentifierFormatter();
    }

    public class DictionaryIdentifierFormatter : IValueFormatter
    {
        public bool CanHandle(object value) => value is IDictionary;

        /// <inheritdoc />
        public string Format(object value, FormattingContext context, FormatChild formatChild)
        {
            var newline = context.UseLineBreaks ? Environment.NewLine : "";
            var padding = new string('\t', context.Depth);

            var result = new StringBuilder($"{newline}{padding}{{");
            foreach (DictionaryEntry entry in (IDictionary)value)
            {
                result.AppendFormat(
                    "[{0}]: {{{1}}},",
                    formatChild("Key", entry.Key),
                    formatChild("Value", entry.Value));
            }

            result.Append($"{newline}{padding}}}");

            return result.ToString();
        }
        
        public static IValueFormatter Instance { get; } = new DictionaryIdentifierFormatter(); 
    }
    
    public static class PersonIdentifierCollectionAssertions
    {
        public static AndWhichConstraint<GenericCollectionAssertions<PersonIdentifierEntity>, PersonIdentifierEntity>
            ContainSingleIdentifierValue(
                this GenericCollectionAssertions<PersonIdentifierEntity> assertions,
                IdentifierDefinition definition,
                string value,
                bool deleted = false
            ) => assertions.ContainSingleMarshalledIdentifierValue(definition, MarshallValue(definition, value), deleted);

        private static AndWhichConstraint<GenericCollectionAssertions<PersonIdentifierEntity>, PersonIdentifierEntity>
            ContainSingleMarshalledIdentifierValue(
                this GenericCollectionAssertions<PersonIdentifierEntity> assertions, 
                IDefinition definition,
                string marshalledValue, 
                bool deleted)
        {
            return assertions.ContainSingle(
                _ => _.Value == marshalledValue && _.TypeName == definition.SystemName &&
                     (deleted ? _.Deleted != null : _.Deleted == null));
        }

        private static string MarshallValue(IdentifierDefinition definition, string value)
        {
            return definition.CreateXmlMarshaller()
                .MarshallToXml(Identifiers.PersonIdentifier(value, definition))
                .ToString(SaveOptions.DisableFormatting);
        }

        public static IIdentifierXmlMarhsaller<PersonIdentifier, IdentifierDefinition> CreateXmlMarshaller(this IdentifierDefinition definition)
        {
            var creator = new IdentifierXmlMarshallerCreator<PersonIdentifier, IdentifierDefinition>();
            definition.Accept(creator);
            return creator.Marshallers.Values.Single();
        }

        public static AndWhichConstraint<GenericCollectionAssertions<PersonIdentifierEntity>, PersonIdentifierEntity>
            ContainSingleIdentifierValue(
                this GenericCollectionAssertions<PersonIdentifierEntity> assertions,
                PersonIdentifier identifier,
                bool deleted = false
            )
        {
            return assertions.ContainSingleMarshalledIdentifierValue(
                identifier.Definition,
                identifier.Definition.CreateXmlMarshaller().MarshallToXml(identifier)
                    .ToString(SaveOptions.DisableFormatting),
                deleted);
        }
    }
}