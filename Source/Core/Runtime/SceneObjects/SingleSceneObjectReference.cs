using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VRBuilder.Core.Configuration;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Step inspector reference to a single <see cref="ISceneObject"/>.
    /// </summary>
    [DataContract(IsReference = true)]
    public class SingleSceneObjectReference : SingleSceneReference<ISceneObject>
    {
        protected override ISceneObject DetermineValue(ISceneObject cached)
        {
            if (IsEmpty())
            {
                return null;
            }

            ISceneObject value = cached;

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

            if (sceneObjects.Count() > 0)
            {
                value = sceneObjects.First();
            }

            return value;
        }
    }
}
