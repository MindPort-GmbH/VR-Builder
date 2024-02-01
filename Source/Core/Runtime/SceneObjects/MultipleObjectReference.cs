using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract(IsReference = true)]
    public abstract class MultipleObjectReference<T> : SceneObjectTag<T> where T : class
    {
        private IEnumerable<T> cachedValue;

        public IEnumerable<T> Values
        {
            get
            {
                cachedValue = DetermineValue(cachedValue);
                return cachedValue;
            }
        }

        protected abstract IEnumerable<T> DetermineValue(IEnumerable<T> cachedValue);

        internal override bool AllowMultipleValues => true;


        public static implicit operator List<T>(MultipleObjectReference<T> reference)
        {
            return reference.Values.ToList();
        }

        public MultipleObjectReference()
        {
        }

        public MultipleObjectReference(Guid guid) : base(guid)
        {
        }
    }
}