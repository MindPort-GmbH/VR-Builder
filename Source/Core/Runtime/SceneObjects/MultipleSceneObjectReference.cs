using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VRBuilder.Core.Configuration;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract(IsReference = true)]
    public class MultipleSceneObjectReference : MultipleObjectReference<ISceneObject>
    {
        public MultipleSceneObjectReference() : base()
        {
        }

        public MultipleSceneObjectReference(Guid guid) : base(guid)
        {
        }

        protected override IEnumerable<ISceneObject> DetermineValue(IEnumerable<ISceneObject> cachedValue)
        {
            if (Guid == null || Guid == Guid.Empty)
            {
                return null;
            }

            IEnumerable<ISceneObject> value = cachedValue;

            // If value exists, return it.
            if (value != null)
            {
                return value;
            }

            value = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(Guid);
            return value;
        }
    }
}
