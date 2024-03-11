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

        public void SetGuid(Guid guid)
        {
            Guid = guid;
        }

        /// <summary>
        /// Checks if the serialized GUID is valid.
        /// </summary>
        /// <param name="serializedGuid">The serialized GUID to check.</param>
        /// <returns>True if the serialized GUID is not null and has a length of 16; otherwise, false.</returns>
        public bool IsValid()
        {
            return SerializableGuid.IsValid(this);
        }

        public static bool IsValid(SerializableGuid serializableGuid)
        {
            return serializableGuid != null && serializableGuid.serializedGuid != null && serializableGuid.serializedGuid.Length == 16;
        }
        public override string ToString()
        {
            if (serializedGuid == null || serializedGuid.Length == 0)
            {
                return "null";
            }
            else if (serializedGuid.Length != 16)
            {
                return "invalid";
            }
            else
            {
                return Guid.ToString();
            }
        }

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
