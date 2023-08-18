using System.Runtime.Serialization;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract]
    public struct ScenePropertyReferenceOrTag<T> where T : class, ISceneObjectProperty
    {
        [DataMember]
        public ScenePropertyReference<T> ScenePropertyReference { get; set; }

        [DataMember]
        public SceneObjectTag<T> Tag { get; set; }

        [DataMember]
        public bool IsTagSelected { get; set; }
    }
}
