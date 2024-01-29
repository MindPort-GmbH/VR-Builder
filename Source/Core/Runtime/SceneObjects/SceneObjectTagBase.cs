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
        /// List of guids, each Guid is a reference to a <seealso cref="SceneObjectTags.Tag"/>.
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

        internal abstract bool AllowMultipleValues { get; }

        public SceneObjectTagBase()
        {
            Guids = new List<Guid>();
        }

        public SceneObjectTagBase(Guid guid)
        {
            // TODO we should not call SceneObjectTagBase with Guid.Empty
            if (guid == null || guid == Guid.Empty)
            {
                Guids = new List<Guid>();
            }
            else
            {
                Guids = new List<Guid> { guid };
            }
        }

        /// <inheritdoc />
        public virtual bool IsEmpty()
        {
            return Guid == null || Guid == Guid.Empty;
        }
    }
}
