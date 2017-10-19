﻿using System.Collections.Generic;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public class AccessControlList : Entity, IAccessControlList
    {
        public virtual ICollection<AccessControlEntry> Permissions { get; protected set; } = new List<AccessControlEntry>();
        /// <inheritdoc />
        IEnumerable<IAccessControlEntry> IAccessControlList.Permissions => Permissions;

        public virtual INHibernateSecurable Owner { get; set; }
    }
}