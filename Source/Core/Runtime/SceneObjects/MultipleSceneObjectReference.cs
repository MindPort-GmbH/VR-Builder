using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract(IsReference = true)]
    public class MultipleSceneObjectReference : MultipleObjectReference<ISceneObject>
    {
        public override IEnumerable<ISceneObject> Values => throw new System.NotImplementedException();
    }
}
