﻿using System;
using System.Runtime.CompilerServices;
using CHC.Consent.Common;
using CHC.Consent.Common.Infrastructure.Data;

[assembly:InternalsVisibleTo("CHC.Consent.EFCore.Tests")]

namespace CHC.Consent.EFCore.Entities
{
    /// <summary>
    /// Stored (and allocates) Ids for people
    /// </summary>
    public class PersonEntity : IEntity
    {
        public virtual long Id { get; set; }
        
        public static implicit operator PersonIdentity(PersonEntity entity)
        {
            return entity == null ? null : new PersonIdentity( entity.Id );
        }
    }
}