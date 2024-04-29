using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Step inspector reference to multiple <see cref="ISceneObjectProperty"/>s.
    /// </summary>    
    [DataContract(IsReference = true)]
    public class MultipleScenePropertyReference<T> : MultipleSceneReference<T> where T : class, ISceneObjectProperty
    {
        /// <inheritdoc/>
        protected override IEnumerable<T> DetermineValue(IEnumerable<T> cachedValue)
        {
            if (RuntimeConfigurator.Exists == false || IsEmpty())
            {
                return new List<T>();
            }

            IEnumerable<T> value = cachedValue;

            // If value exists, return it.
            if (value != null)
            {
                return value;
            }

            value = new List<T>();

            foreach (Guid guid in Guids)
            {
                value = value.Concat(RuntimeConfigurator.Configuration.SceneObjectRegistry.GetProperties<T>(guid)).Distinct();
            }

            return value;
        }

        public MultipleScenePropertyReference() : base() { }
        public MultipleScenePropertyReference(Guid guid) : base(guid) { }
        public MultipleScenePropertyReference(IEnumerable<Guid> guids) : base(guids) { }
    }
}
