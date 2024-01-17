using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract(IsReference = true)]
    public class MultipleScenePropertyReference<T> : MultipleObjectReference<T> where T : class, ISceneObjectProperty
    {
        public override IEnumerable<T> Values => throw new System.NotImplementedException();

        public MultipleScenePropertyReference()
        {
        }

        public MultipleScenePropertyReference(Guid guid) : base(guid)
        {
        }
    }
}
