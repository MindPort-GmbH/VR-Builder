using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Step inspector reference to a single <see cref="ISceneObjectProperty"/>.
    /// </summary>
    [DataContract(IsReference = true)]
    public class SingleScenePropertyReference<T> : SingleSceneReference<T> where T : class, ISceneObjectProperty
    {
        /// <inheritdoc />
        protected override T DetermineValue(T cachedValue)
        {
            if (RuntimeConfigurator.Exists == false || IsEmpty())
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

            IEnumerable<T> properties = new List<T>();

            foreach (Guid guid in Guids)
            {
                properties = properties.Concat(RuntimeConfigurator.Configuration.SceneObjectRegistry.GetProperties<T>(guid)).Distinct();
            }

            return properties.FirstOrDefault();
        }

        public SingleScenePropertyReference() : base() { }
        public SingleScenePropertyReference(Guid guid) : base(guid) { }
        public SingleScenePropertyReference(IEnumerable<Guid> guids) : base(guids) { }
    }
}
