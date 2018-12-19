﻿using System.Collections.Generic;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CHC.Consent.Api.Infrastructure
{
    public static class IdentifierRegistryJsonExtensions
    {
        
        public static JsonSerializerSettings CreateSerializerSettings(this ITypeRegistry<CaseIdentifier> identifierRegistry)
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = { new StringEnumConverter() }
            };
        }
        
        public static JsonSerializerSettings CreateSerializerSettings(this IdentifierDefinitionRegistry identifierRegistry)
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new IdentifierRegistrySerializationBinder(identifierRegistry),
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = { new StringEnumConverter(), new PersonIdentifierConverter(identifierRegistry) }
            };
        }
    }
}