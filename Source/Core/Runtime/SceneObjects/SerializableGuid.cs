using System;
using UnityEngine;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Serializable version of a <see cref="System.Guid"/>.
    /// </summary>
    /// <remarks>
    /// If not working with the Unity Editor better use Newtonsoft.Json.
    /// It has a implementation to serialize <see cref="System.Guid"/>.
    /// </remarks>
    [Serializable]
    public class SerializableGuid : IEquatable<SerializableGuid>, IEquatable<Guid>
    {
        [SerializeField]
        private byte[] serializedGuid = Guid.NewGuid().ToByteArray();

        /// <summary>
        /// Converts the <see cref="System.Guid"/> to byte[] for serialization.
        /// </summary>
        public Guid Guid
        {
            get => new Guid(serializedGuid);
            set => serializedGuid = value.ToByteArray();
        }

        public SerializableGuid(byte[] bytes)
        {
            serializedGuid = bytes;
        }

        public SerializableGuid(Guid guid)
        {
            Guid = guid;
        }


        /// <summary>
        /// Sets the Guid value for the SerializableGuid.
        /// </summary>
        /// <param name="guid">The Guid value to set.</param>
        public void SetGuid(Guid guid)
        {
            Guid = guid;
        }

        /// <summary>
        /// Checks if the serialized Guid is valid.
        /// </summary>
        /// <param name="serializedGuid">The serialized Guid to check.</param>
        /// <returns><c>true</c> if the serialized Guid is not null and has a length of 16; otherwise, <c>false</c>.</returns>
        public bool IsValid()
        {
            return SerializableGuid.IsValid(this);
        }

        /// <summary>
        /// Checks if a <see cref="SerializableGuid"/> is valid.
        /// </summary>
        /// <param name="serializableGuid">The <see cref="SerializableGuid"/> to check.</param>
        /// <returns><c>true</c> if the <see cref="SerializableGuid"/> and <see cref="serializedGuid"/> is not null, and has a length of 16; otherwise, <c>false</c>.</returns>
        public static bool IsValid(SerializableGuid serializableGuid)
        {
            return serializableGuid != null && serializableGuid.serializedGuid != null && serializableGuid.serializedGuid.Length == 16;
        }

        /// <summary>
        /// Returns a string representation of the current SerializableGuid object.
        /// </summary>
        /// <returns>
        /// A string representation of the current SerializableGuid object.
        /// If the serializedGuid is null or empty, it returns "null".
        /// If the serializedGuid length is not equal to 16, it returns "invalid".
        /// Otherwise, it returns the string representation of the Guid.
        /// </returns>
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

        /// <summary>
        /// Determines whether the current <see cref="SerializableGuid"/> object is equal to another <see cref="SerializableGuid"/> object.
        /// </summary>
        /// <param name="other">The <see cref="SerializableGuid"/> to compare with the current object.</param>
        /// <returns><c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(SerializableGuid other)
        {
            return other != null && Guid.Equals(other.Guid);
        }

        /// <summary>
        /// Determines whether the current SerializableGuid object is equal to the specified Guid.
        /// </summary>
        /// <param name="otherGuid">The Guid to compare with the current SerializableGuid object.</param>
        /// <returns>true if the current SerializableGuid object is equal to the specified Guid; otherwise, false.</returns>
        public bool Equals(Guid otherGuid)
        {
            return Guid.Equals(otherGuid);
        }

        /// <summary>
        /// Determines whether the current <see cref="SerializableGuid"/> object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <remarks>This is used in hash-based collections, like Dictionary.</remarks>
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
