﻿using CHC.Consent.Api.Features.Consent;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Identifiers;
using CHC.Consent.Common.Infrastructure;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.Tests.Consent
{
    public class JsonSerializationTests
    {
        private readonly ITestOutputHelper output;
        private TypeRegistry<CaseIdentifier, CaseIdentifierAttribute> registry;
        private JsonSerializerSettings serializerSettings;

        /// <inheritdoc />
        public JsonSerializationTests(ITestOutputHelper output)
        {
            this.output = output;
            registry = new TypeRegistry<CaseIdentifier, CaseIdentifierAttribute>();
            registry.Add<PregnancyNumberIdentifier>();
            serializerSettings = registry.CreateSerializerSettings();
        }

        [Fact]
        public void WhatDoesJsonLookLike()
        {
            
            output.WriteLine(
                JsonConvert.SerializeObject(
                    new ConsentSpecification
                    {
                        CaseId = new CaseIdentifier[] {new PregnancyNumberIdentifier("testing, testing, 1..2..3")}

                    },
                    serializerSettings)
            );
        }

        [Fact]
        public void CanDeserializeConsentSpecification()
        {
            var roundTripped = JsonConvert.DeserializeObject<ConsentSpecification>(
                JsonConvert.SerializeObject(
                    new ConsentSpecification
                    {
                        CaseId = new CaseIdentifier[] {new PregnancyNumberIdentifier("testing, testing, 1..2..3")}

                    },
                    serializerSettings),
                serializerSettings
            );
            
            Assert.NotNull(roundTripped);
            var identifier = Assert.Single(roundTripped.CaseId);
            var prenancyNumber = Assert.IsType<PregnancyNumberIdentifier>(identifier);
            Assert.Equal("testing, testing, 1..2..3", prenancyNumber.Value);
        }
    }
}