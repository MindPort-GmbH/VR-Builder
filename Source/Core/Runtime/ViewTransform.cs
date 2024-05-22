using System.Runtime.Serialization;
using UnityEngine;

namespace VRBuilder.Core
{
    [DataContract(IsReference = true)]
    public class ViewTransform
    {
        [DataMember]
        public Vector3 Position { get; set; }

        [DataMember]
        public Vector3 Scale { get; set; }

        public ViewTransform(Vector3 position, Vector3 scale)
        {
            Position = position;
            Scale = scale;
        }
    }
}
