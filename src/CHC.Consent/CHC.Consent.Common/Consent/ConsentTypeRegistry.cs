﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;
using JetBrains.Annotations;

namespace CHC.Consent.Common.Consent
{
    [UsedImplicitly]
    public class ConsentTypeRegistry : ITypeRegistry
    {
        public ConsentTypeRegistry(
            ITypeRegistry<CaseIdentifier> consentIdentifiers, 
            ITypeRegistry<Evidence> evidences,
            ITypeRegistry<IPersonIdentifier> personIdentifiers)
        {
            Registries = new List<ITypeRegistry> {consentIdentifiers, evidences, personIdentifiers};
        }

        private IEnumerable<ITypeRegistry> Registries { get; }
        

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerator<ClassMapping> GetEnumerator() => Registries.SelectMany(_ => _).GetEnumerator();

        /// <inheritdoc />
        public bool TryGetName(Type type, out string name)
        {
            foreach (var registry in Registries)
            {
                if (registry.TryGetName(type, out name)) return true;
            }

            name = null;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetType(string name, out Type type)
        {
            foreach (var registry in Registries)
            {
                if (registry.TryGetType(name, out type)) return true;
            }

            type = null;
            return false;
        }


        /// <inheritdoc />
        public Type this[string name] =>
            TryGetType(name, out var type)
                ? type
                : throw new KeyNotFoundException($"Cannot find type named '{name}'");

        /// <inheritdoc />
        public string this[Type type] =>
            TryGetName(type, out var name)
                ? name
                : throw new KeyNotFoundException($"Cannot find name for type '{type}'");
    }
}