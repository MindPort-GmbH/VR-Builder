using System;
using System.Runtime.Serialization;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract(IsReference = true)]
    public sealed class SceneObjectTag<T> : SceneObjectTagBase
    {
        public SceneObjectTag() : base()
        {
        }

        public SceneObjectTag(Guid guid) : base(guid)
        {
        }

        internal override Type GetReferenceType()
        {
            return typeof(T);
        }
    }
}
