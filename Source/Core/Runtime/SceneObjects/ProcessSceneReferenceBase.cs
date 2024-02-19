using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VRBuilder.Core.Runtime.Properties;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Base class for a process reference to one or more objects.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class ProcessSceneReferenceBase : ICanBeEmpty
    {
        /// <summary>
        /// List of guids, each Guid is a reference to a <see cref="Settings.SceneObjectTags.Tag"/>.
        /// </summary>
        [DataMember]
        public virtual List<Guid> Guids { get; set; }

        /// <summary>
        /// Returns the type this reference is associated with.
        /// </summary>        
        internal abstract Type GetReferenceType();

        /// <summary>
        /// If true, this reference can return multiple values.
        /// </summary>
        internal abstract bool AllowMultipleValues { get; }

        public ProcessSceneReferenceBase()
        {
            Guids = new List<Guid>();
        }

        public ProcessSceneReferenceBase(Guid guid)
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

        public ProcessSceneReferenceBase(IEnumerable<Guid> guids)
        {
            if (guids == null)
            {
                Guids = new List<Guid>();
            }
            else
            {
                Guids = guids.ToList();
            }
        }

        /// <inheritdoc />
        public virtual bool IsEmpty()
        {
            return Guids == null || Guids.Count() == 0 || Guids.All(guid => guid == Guid.Empty);
        }
    }
}
