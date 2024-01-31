using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VRBuilder.Core.Runtime.Properties;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Base class for step inspector references to tags.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class SceneObjectTagBase : ICanBeEmpty
    {
        /// <summary>
        /// The first guid added or Guid.Empty. 
        /// Each Guid is a reference to a <seealso cref="SceneObjectTags.Tag"/>.
        /// </summary>
        [DataMember]
        public virtual Guid Guid
        {
            get
            {
                if (Guids == null || Guids.Count == 0)
                {
                    return Guid.Empty;
                }
                return Guids[0];
            }
        }

        /// <summary>
        /// List of guids, each Guid is a reference to a <see cref="Settings.SceneObjectTags.Tag"/>.
        /// </summary>
        [DataMember]
        public virtual List<Guid> Guids { get; set; }

        /// <summary>
        /// The inspector type which has been selected.
        /// </summary>
        [DataMember]
        public virtual InspectorType InspectorType { get; set; }

        /// <summary>
        /// Returns the type this tag is associated with.
        /// </summary>        
        internal abstract Type GetReferenceType();

        internal abstract int MaxValuesAllowed { get; }

        /// <summary>
        /// Base class for behaviors and conditions to use <see cref="Settings.SceneObjectTags.Tag"/> for referencing <see cref="ProcessSceneObject"/>.
        /// </summary>
        public SceneObjectTagBase()
        {
            Guids = new List<Guid>();
        }

        /// <summary>
        /// Base class for behaviors and conditions to use <see cref="Settings.SceneObjectTags.Tag"/> for referencing <see cref="ProcessSceneObject"/>.
        /// </summary>
        /// <param name="guid">Guid representing a <see cref="Settings.SceneObjectTags.Tag"/>. This usually will be <seealso cref="Guid.Empty"/> except when running automated Tests</param>
        public SceneObjectTagBase(Guid guid)
        {
            if (guid == Guid.Empty)
            {
                Guids = new List<Guid>();
            }
            else
            {
                Guids = new List<Guid> { guid };
            }
        }


        /// <summary>
        /// Base class for behaviors and conditions to use <see cref="Settings.SceneObjectTags.Tag"/> for referencing <see cref="ProcessSceneObject"/>.
        /// </summary>
        /// <param name="guids">Representing a list of <see cref="Settings.SceneObjectTags.Tag"/>. This main reason for existence is for running automated Tests</param>
        public SceneObjectTagBase(List<Guid> guids)
        {
            if (guids == null)
            {
                Guids = new List<Guid>();
            }
            else
            {
                Guids = guids;
            }
        }

        /// <inheritdoc />
        public virtual bool IsEmpty()
        {
            return Guids == null || Guids.Count == 0 || Guids[0] == Guid.Empty;
        }
    }
}
