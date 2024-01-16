using System;
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
        /// The guid representing the tag.
        /// </summary>
        [DataMember]
        public virtual Guid Guid { get; set; }

        /// <summary>
        /// The inspector type which has been selected.
        /// </summary>
        [DataMember]
        public virtual InspectorType InspectorType { get; set; }

        /// <summary>
        /// Returns the type this tag is associated with.
        /// </summary>        
        internal abstract Type GetReferenceType();

        public SceneObjectTagBase()
        {
        }

        public SceneObjectTagBase(Guid guid)
        {
            Guid = guid;
        }

        /// <inheritdoc />
        public virtual bool IsEmpty()
        {
            return Guid == null || Guid == Guid.Empty;
        }
    }
}
