using System;
using System.Runtime.Serialization;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract(IsReference = true)]
    public abstract class SingleObjectReference<T> : SceneObjectTag<T> where T : class
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

        public static implicit operator T(SingleObjectReference<T> reference)
        {
            return reference.Value;
        }

        protected abstract T DetermineValue(T cachedValue);

        public SingleObjectReference()
        {
        }

        public SingleObjectReference(Guid guid) : base(guid)
        {
        }
    }
}
