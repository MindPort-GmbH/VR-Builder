using System;
using UnityEngine;

namespace VRBuilder.Core.SceneObjects
{
    [Serializable]
    public class SerializableGuid : IEquatable<SerializableGuid>, IEquatable<Guid>
    {
        [SerializeField]
        private byte[] serializedGuid = Guid.NewGuid().ToByteArray();

        public Guid Guid
        {
            get => new Guid(serializedGuid);
            set => serializedGuid = value.ToByteArray();
        }

        public SerializableGuid(byte[] bytes)
        {
            serializedGuid = bytes;
        }

        public override string ToString() => Guid.ToString();

        public bool Equals(SerializableGuid other)
        {
            return other != null && Guid.Equals(other.Guid);
        }

        public bool Equals(Guid otherGuid)
        {
            return Guid.Equals(otherGuid);
        }

        public override bool Equals(object obj)
        {
            if (obj is SerializableGuid otherSerializableGuid)
            {
                return Equals(otherSerializableGuid);
            }
            if (obj is Guid otherGuid)
            {
                return Equals(otherGuid);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        public static bool operator ==(SerializableGuid left, SerializableGuid right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(SerializableGuid left, SerializableGuid right)
        {
            return !(left == right);
        }
    }
}
