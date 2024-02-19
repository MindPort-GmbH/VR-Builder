using System;
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

            value = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(Guid).FirstOrDefault();
            return value;
        }
    }
}
