using System;
using System.Runtime.Serialization;
using VRBuilder.Core.Runtime.Properties;

namespace VRBuilder.Core.SceneObjects
{
    [Serializable]
    [DataContract]
    public sealed class SceneObjectTag : ICanBeEmpty
    {
        [DataMember]
        public Guid Guid { get; set; }

        [DataMember]
        public Type PropertyType { get; set; }

        public SceneObjectTag()
        {
        }

        public SceneObjectTag(Guid guid, Type propertyType)
        {
            Guid = guid;
            PropertyType = propertyType;
        }

        public bool IsEmpty()
        {
            return Guid == null || Guid == Guid.Empty;
        }
    }
}
