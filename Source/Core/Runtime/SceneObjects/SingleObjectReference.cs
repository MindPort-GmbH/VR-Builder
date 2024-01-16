using System;
using System.Runtime.Serialization;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract(IsReference = true)]
    public abstract class SingleObjectReference<T> : SceneObjectTagBase where T : class
    {
        private T cachedValue;

        public T Value
        {
            get
            {
                cachedValue = null; // Ideally, we don't want to cache this value.
                cachedValue = DetermineValue(cachedValue);
                return cachedValue;
            }
        }

        internal override bool AllowMultipleValues => false;


        /// <inheritdoc />
        internal override Type GetReferenceType()
        {
            return typeof(T);
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
