using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Exceptions;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract(IsReference = true)]
    public class MultipleScenePropertyReference<T> : MultipleObjectReference<T> where T : class, ISceneObjectProperty
    {
        public MultipleScenePropertyReference() : base()
        {
        }

        public MultipleScenePropertyReference(Guid guid) : base(guid)
        {
        }

        protected override IEnumerable<T> DetermineValue(IEnumerable<T> cachedValue)
        {
            if (Guid == null || Guid == Guid.Empty)
            {
                return null;
            }

            IEnumerable<T> value = cachedValue;

            // If value exists, return it.
            if (value != null)
            {
                return value;
            }

            IEnumerable<ISceneObject> sceneObject = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(Guid);

            // Can't find process object with given UniqueName, value is null.
            if (sceneObject.Count() == 0)
            {
                return value;
            }

            value = null;

            // Allows non-unique referencing system to have guid but no property
            try
            {
                value = sceneObject.Select(so => so.GetProperty<T>());
            }
            catch (PropertyNotFoundException)
            {
            }

            return value;
        }
    }
}
