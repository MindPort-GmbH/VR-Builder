using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract(IsReference = true)]
    public class MultipleScenePropertyReference<T> : MultipleObjectReference<T> where T : class, ISceneObjectProperty
    {
        public MultipleScenePropertyReference()
        {
        }

        public MultipleScenePropertyReference(Guid guid) : base(guid)
        {
        }

        protected override IEnumerable<T> DetermineValue(IEnumerable<T> cachedValue)
        {
            throw new NotImplementedException();
        }
    }
}
