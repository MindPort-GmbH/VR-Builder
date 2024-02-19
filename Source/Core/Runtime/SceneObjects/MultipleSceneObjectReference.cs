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
        protected override IEnumerable<ISceneObject> DetermineValue(IEnumerable<ISceneObject> cachedValue)
        {
            if (IsEmpty())
            {
                return null;
            }

            IEnumerable<ISceneObject> value = cachedValue;

            // If value exists, return it.
            if (value != null)
            {
                return value;
            }

            value = null;

            IEnumerable<ISceneObject> sceneObjects = new List<ISceneObject>();

            foreach (Guid guid in Guids)
            {
                sceneObjects = sceneObjects.Concat(RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(guid));
            }

            if (sceneObjects.Count() > 0)
            {
                value = sceneObjects;
            }

            return value;
        }

        public MultipleSceneObjectReference() : base() { }
        public MultipleSceneObjectReference(Guid guid) : base(guid) { }
        public MultipleSceneObjectReference(IEnumerable<Guid> guids) : base(guids) { }
    }
}
