using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Base class for a process reference to one or more objects.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class ProcessSceneReferenceBase : ICanBeEmpty
    {
        /// <summary>
        /// List of guids, each Guid is a reference to a <see cref="Settings.SceneObjectGroups.SceneObjectGroup"/>.
        /// </summary>
        public IReadOnlyList<Guid> Guids => guids;

        [DataMember]
        private List<Guid> guids;

        /// <summary>
        /// Returns the type this reference is associated with.
        /// </summary>        
        internal abstract Type GetReferenceType();

        /// <summary>
        /// If true, this reference can return multiple values.
        /// </summary>
        internal abstract bool AllowMultipleValues { get; }

        /// <summary>
        /// Returns true if the reference contains a non-null value.
        /// </summary>        
        public abstract bool HasValue();

        /// <summary>
        /// Adds the specified guid to this reference.
        /// </summary>        
        public void AddGuid(Guid guid)
        {
            guids.Add(guid);
        }

        /// <summary>
        /// Removes the specified guid from this reference.
        /// </summary>
        public bool RemoveGuid(Guid guid)
        {
            return guids.Remove(guid);
        }

        /// <summary>
        /// Resets the guids on this reference to the specified value.
        /// </summary>        
        public void ResetGuids(IEnumerable<Guid> newGuids = null)
        {
            if (newGuids == null)
            {
                guids.Clear();
            }
            else
            {
                guids = newGuids.ToList();
            }
        }

        public ProcessSceneReferenceBase()
        {
            guids = new List<Guid>();
        }

        public ProcessSceneReferenceBase(Guid guid)
        {
            if (guid == Guid.Empty)
            {
                guids = new List<Guid>();
            }
            else
            {
                guids = new List<Guid> { guid };
            }
        }

        public ProcessSceneReferenceBase(IEnumerable<Guid> guids)
        {
            if (guids == null)
            {
                this.guids = new List<Guid>();
            }
            else
            {
                this.guids = guids.ToList();
            }
        }

        /// <inheritdoc />
        public virtual bool IsEmpty()
        {
            return Guids == null || Guids.Count() == 0 || Guids.All(guid => guid == Guid.Empty);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            ProcessSceneReferenceBase sceneReference = obj as ProcessSceneReferenceBase;

            if (sceneReference == null)
            {
                return false;
            }

            return GetType() == sceneReference.GetType() &&
                Guids.OrderBy(guid => guid).SequenceEqual(sceneReference.Guids.OrderBy(guid => guid)) &&
                AllowMultipleValues == sceneReference.AllowMultipleValues;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Guids, AllowMultipleValues);
        }

        public static bool operator ==(ProcessSceneReferenceBase left, ProcessSceneReferenceBase right)
        {
            if ((object)left == null)
            {
                return (object)right == null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(ProcessSceneReferenceBase left, ProcessSceneReferenceBase right)
        {
            if ((object)left == null)
            {
                return (object)right != null;
            }

            return left.Equals(right) == false;
        }
    }
}
