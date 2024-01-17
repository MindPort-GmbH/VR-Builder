using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract(IsReference = true)]
    public class MultipleSceneObjectReference : MultipleObjectReference<ISceneObject>
    {
        public MultipleSceneObjectReference()
        {
        }

        public MultipleSceneObjectReference(Guid guid) : base(guid)
        {
        }

        protected override IEnumerable<ISceneObject> DetermineValue(IEnumerable<ISceneObject> cachedValue)
        {
            throw new NotImplementedException();
        }
    }
}
