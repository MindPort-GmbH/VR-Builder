using System.Runtime.Serialization;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract]
    public struct SceneObjectReferenceOrTag
    {
        [DataMember]
        public SceneObjectReference SceneObjectReference { get; set; }

        [DataMember]
        public SceneObjectTag<ISceneObject> Tag { get; set; }

        [DataMember]
        public bool IsTagSelected { get; set; }
    }
}
