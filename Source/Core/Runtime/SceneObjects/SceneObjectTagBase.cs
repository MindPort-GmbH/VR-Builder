using System;
using System.Runtime.Serialization;
using VRBuilder.Core.Runtime.Properties;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract(IsReference = true)]
    public abstract class SceneObjectTagBase : ICanBeEmpty
    {
        [DataMember]
        public Guid Guid { get; set; }

        public SceneObjectTagBase()
        {
        }

        public SceneObjectTagBase(Guid guid)
        {
            Guid = guid;
        }

        public bool IsEmpty()
        {
            return Guid == null || Guid == Guid.Empty;
        }
    }
}
