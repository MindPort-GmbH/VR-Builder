using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Step inspector reference to multiple objects.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class MultipleSceneReference<T> : ProcessSceneReference<T> where T : class
    {
        /// <summary>
        /// The referenced values.
        /// </summary>
        public IEnumerable<T> Values
        {
            get
            {
                return DetermineValue(null);
            }
        }

        /// <summary>
        /// Determine the objects referenced by this scene reference.
        /// </summary>
        protected abstract IEnumerable<T> DetermineValue(IEnumerable<T> cachedValue);

        /// <inheritdoc/>
        internal override bool AllowMultipleValues => true;

        /// <inheritdoc/>
        public override bool HasValue()
        {
            return IsEmpty() == false && Values != null;
        }

        public static implicit operator List<T>(MultipleSceneReference<T> reference)
        {
            return reference.Values.ToList();
        }

        public override string ToString()
        {
            if (Guids.Count == 1 && SceneObjectGroups.Instance.GroupExists(Guids.First()))
            {
                return $"objects in '{SceneObjectGroups.Instance.GetLabel(Guids.First())}'";
            }

            if (HasValue() == false)
            {
                return "[NULL]";
            }

            if (Values.Count() == 1)
            {
                return $"'{Values.First()}'";
            }

            return $"{Values.Count()} objects";
        }

        public MultipleSceneReference() : base() { }
        public MultipleSceneReference(Guid guid) : base(guid) { }
        public MultipleSceneReference(IEnumerable<Guid> guids) : base(guids) { }
    }
}
