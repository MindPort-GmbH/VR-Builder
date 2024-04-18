using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Step inspector reference to a single object.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class SingleSceneReference<T> : ProcessSceneReference<T> where T : class
    {
        public T Value
        {
            get
            {
                return DetermineValue(null);
            }
        }

        /// <inheritdoc />
        internal override bool AllowMultipleValues => false;

        /// <inheritdoc />
        public override bool HasValue()
        {
            return IsEmpty() == false && Value != null;
        }

        public override string ToString()
        {
            if (Guids.Count == 1 && SceneObjectGroups.Instance.GroupExists(Guids.First()))
            {
                return $"an object in '{SceneObjectGroups.Instance.GetLabel(Guids.First())}'";
            }

            if (HasValue() == false)
            {
                return "[NULL]";
            }

            return $"'{Value}'";
        }

        /// <summary>
        /// Determine the object referenced by this scene reference.
        /// </summary>
        protected abstract T DetermineValue(T cachedValue);

        public SingleSceneReference() : base() { }
        public SingleSceneReference(Guid guid) : base(guid) { }
        public SingleSceneReference(IEnumerable<Guid> guids) : base(guids) { }
    }
}
