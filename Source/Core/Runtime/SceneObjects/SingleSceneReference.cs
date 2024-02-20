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
    /// <typeparam name="T"></typeparam>
    [DataContract(IsReference = true)]
    public abstract class SingleSceneReference<T> : ProcessSceneReference<T> where T : class
    {
        private T cachedValue;

        public T Value
        {
            get
            {
                cachedValue = DetermineValue(cachedValue);
                return cachedValue;
            }
        }

        internal override bool AllowMultipleValues => false;

        public static implicit operator T(SingleSceneReference<T> reference)
        {
            return reference.Value;
        }

        public override string ToString()
        {
            if (IsEmpty())
            {
                return "[NULL]";
            }

            if (Guids.Count() == 1 && SceneObjectTags.Instance.TagExists(Guids.First()))
            {
                return $"object of type '{SceneObjectTags.Instance.GetLabel(Guids.First())}'";
            }

            return $"'{Value}'";
        }

        protected abstract T DetermineValue(T cachedValue);

        public SingleSceneReference() : base() { }
        public SingleSceneReference(Guid guid) : base(guid) { }
        public SingleSceneReference(IEnumerable<Guid> guids) : base(guids) { }
    }
}
