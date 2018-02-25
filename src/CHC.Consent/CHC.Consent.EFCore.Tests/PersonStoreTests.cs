﻿using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.Testing.Utils;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.EFCore.Tests
{
    [Collection(DatabaseCollection.Name)]
    public class PersonStoreTests : DbTests
    {
        public PersonStoreTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) 
            : base(outputHelper, fixture)
        {
        }

        [Fact]
        public void CanSaveAPerson()
        {
            var store = new PersonStore(Context.People);

            var person = store.Add(new Person {DateOfBirth = 3.April(2018)});

            Context.SaveChanges();

            Assert.NotEqual(0, person.Id);
        }

        [Fact]
        public void CanQueryPeople()
        {
            Context.People.AddRange(
                new PersonEntity { NhsNumber = "11-11-111"},
                new PersonEntity { NhsNumber = "22-22-222"},
                new PersonEntity { NhsNumber = "33-33-333"},
                new PersonEntity { NhsNumber = "44-44-444"},
                new PersonEntity { NhsNumber = "55-55-555"});
            Context.SaveChanges();

            var store = new PersonStore(Context.People);

            var foundPeople = store.Where(_ => _.NhsNumber == "33-33-333").ToArray();

            var person = Assert.Single(foundPeople);
            Assert.NotNull(person);
            Assert.Equal("33-33-333", person.NhsNumber);
        }

        [Fact]
        public void SavesHosptialNumbers()
        {
            var person = new Person();
            person.AddHospitalNumber("HOSPITAL");
            var saved = new PersonStore(Context.People).Add(person);
            Context.SaveChanges();

            var hospitalNumber =
                CreateNewContextInSameTransaction()
                    .Set<BradfordHospitalNumberEntity>()
                    .SingleOrDefault(_ => _.Person.Id == saved.Id);
            
            Assert.NotNull(hospitalNumber);
            Assert.Equal("HOSPITAL", hospitalNumber.HospitalNumber);
        }

        [Fact]
        public void HosptialNumbersAreLoaded()
        {
            var person = new PersonEntity();
            person.AddHospitalNumber("LOADED");
            Context.People.Add(person);
            Context.SaveChanges();

            
            var store = new PersonStore(CreateNewContextInSameTransaction().Set<PersonEntity>());

            var first = store.FirstOrDefault(_ => _.Id == person.Id);

            Assert.NotNull(first);
            Assert.Equal("LOADED", Assert.Single(first.BradfordHospitalNumbers));
            
        }
    }
}