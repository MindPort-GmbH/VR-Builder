using System;
using System.Linq;
using System.Runtime.Serialization;
using VRBuilder.Core.Configuration;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract(IsReference = true)]
    public class SingleSceneObjectReference : SingleObjectReference<ISceneObject>
    {
        public SingleSceneObjectReference()
        {
        }

        public SingleSceneObjectReference(Guid guid) : base(guid)
        {
        }

        protected override ISceneObject DetermineValue(ISceneObject cached)
        {
            if (Guid == null || Guid == Guid.Empty)
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
