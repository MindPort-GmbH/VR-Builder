// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System;
using System.Runtime.Serialization;

namespace VRBuilder.Core.SceneObjects
{
    //TODO this class should not be used any more and is obsolete
    /// <summary>
    /// Simple container for UniqueName.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class UniqueNameReference
    {
        /// <summary>
        /// Serializable unique name of referenced object.
        /// </summary>
        [DataMember]
        public virtual string UniqueName { get; set; }

        /// <summary>
        /// Converts the stored unique name to a guid.
        /// </summary>
        public virtual Guid Guid
        {
            get
            {
                Guid guid = Guid.Empty;
                Guid.TryParse(UniqueName, out guid);
                return guid;
            }
            set
            {
                UniqueName = value.ToString();
            }
        }

        protected UniqueNameReference() { }

        protected UniqueNameReference(string uniqueName)
        {
            UniqueName = uniqueName;
        }

        internal abstract Type GetReferenceType();
    }
}
