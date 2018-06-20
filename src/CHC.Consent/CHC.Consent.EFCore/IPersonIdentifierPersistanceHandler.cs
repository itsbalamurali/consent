﻿using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore
{
    public interface IPersonIdentifierPersistanceHandler
    {
        bool Update(PersonEntity person, IEnumerable<IPersonIdentifier> value, IStoreProvider stores);
        IEnumerable<IPersonIdentifier> GetIdentifiers(PersonEntity person, IStoreProvider stores);

        IQueryable<PersonEntity> Filter(
            IQueryable<PersonEntity> people, IPersonIdentifier identifier, IStoreProvider storeProvider);
    }

    public interface IPersonIdentifierPersistanceHandler<TIdentifier> where TIdentifier : IPersonIdentifier
    {
        IQueryable<PersonEntity> Filter(
            IQueryable<PersonEntity> people, TIdentifier value, IStoreProvider stores);

        IEnumerable<TIdentifier> Get(PersonEntity person, IStoreProvider stores);
        
        bool Update(PersonEntity person, TIdentifier[] value, IStoreProvider stores);
    }
}