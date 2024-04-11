using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VRBuilder.Core.Configuration;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Step inspector reference to multiple <see cref="ISceneObject"/>s.
    /// </summary>
    [DataContract(IsReference = true)]
    public class MultipleSceneObjectReference : MultipleSceneReference<ISceneObject>
    {
        /// <inheritdoc />
        protected override IEnumerable<ISceneObject> DetermineValue(IEnumerable<ISceneObject> cachedValue)
        {
            if (RuntimeConfigurator.Exists == false || IsEmpty())
            {
                return new List<ISceneObject>();
            }

            IEnumerable<ISceneObject> value = cachedValue;

            // If value exists, return it.
            if (value != null)
            {
                return value;
            }

            value = new List<ISceneObject>();

            foreach (Guid guid in Guids)
            {
                value = value.Concat(RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid)).Distinct();
            }

            return value;
        }

        public MultipleSceneObjectReference() : base() { }
        public MultipleSceneObjectReference(Guid guid) : base(guid) { }
        public MultipleSceneObjectReference(IEnumerable<Guid> guids) : base(guids) { }
    }
}
