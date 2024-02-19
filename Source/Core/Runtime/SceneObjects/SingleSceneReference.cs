using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

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

        protected abstract T DetermineValue(T cachedValue);

        public SingleSceneReference() : base() { }
        public SingleSceneReference(Guid guid) : base(guid) { }
        public SingleSceneReference(IEnumerable<Guid> guids) : base(guids) { }
    }
}
