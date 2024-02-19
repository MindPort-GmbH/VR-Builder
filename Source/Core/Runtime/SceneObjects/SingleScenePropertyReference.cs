using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Exceptions;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Step inspector reference to a single <see cref="ISceneObjectProperty"/>.
    /// </summary>
    [DataContract(IsReference = true)]
    public class SingleScenePropertyReference<T> : SingleSceneReference<T> where T : class, ISceneObjectProperty
    {
        public static implicit operator T(SingleScenePropertyReference<T> reference)
        {
            return reference.Value;
        }

        protected override T DetermineValue(T cachedValue)
        {
            if (IsEmpty())
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

            IEnumerable<ISceneObject> sceneObjects = new List<ISceneObject>();

            foreach (Guid guid in Guids)
            {
                sceneObjects = sceneObjects.Concat(RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(guid));
            }

            // Can't find corresponding process objects, value is null.
            if (sceneObjects.Count() == 0)
            {
                return value;
            }

            value = null;

            // Allows non-unique referencing system to have guid but no property
            try
            {
                value = sceneObjects.First().GetProperty<T>();
            }
            catch (PropertyNotFoundException)
            {
            }

            return value;
        }

        public SingleScenePropertyReference() : base() { }
        public SingleScenePropertyReference(Guid guid) : base(guid) { }
        public SingleScenePropertyReference(IEnumerable<Guid> guids) : base(guids) { }
    }
}
