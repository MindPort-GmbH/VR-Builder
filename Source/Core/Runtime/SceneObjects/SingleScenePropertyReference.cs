using System;
using System.Linq;
using System.Runtime.Serialization;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Exceptions;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract(IsReference = true)]
    public class SingleScenePropertyReference<T> : SingleObjectReference<T> where T : class, ISceneObjectProperty
    {
        public static implicit operator T(SingleScenePropertyReference<T> reference)
        {
            return reference.Value;
        }

        public SingleScenePropertyReference()
        {
        }

        public SingleScenePropertyReference(Guid guid) : base(guid)
        {
        }

        protected override T DetermineValue(T cachedValue)
        {
            if (Guid == null || Guid == Guid.Empty)
            {
                return null;
            }

            T value = cachedValue;

            // If MonoBehaviour was destroyed, nullify the value.
            if (value != null && value.Equals(null))
            {
                value = null;
            }

            // If value exists, return it.
            if (value != null)
            {
                return value;
            }

            ISceneObject sceneObject = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(Guid).FirstOrDefault();

            // Can't find process object with given UniqueName, value is null.
            if (sceneObject == null)
            {
                return value;
            }

            value = null;

            // Allows non-unique referencing system to have guid but no property
            try
            {
                value = sceneObject.GetProperty<T>();
            }
            catch (PropertyNotFoundException)
            {
            }

            return value;
        }
    }
}
