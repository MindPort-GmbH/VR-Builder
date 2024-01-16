using System;
using System.Runtime.Serialization;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Step inspector reference to a <see cref="SceneObjectTagBase"/> requiring a specific property.
    /// </summary>    
    [DataContract(IsReference = true)]
    public class SceneObjectTag<T> : SceneObjectTagBase where T : class
    {
        public SceneObjectTag() : base()
        {
        }

        public SceneObjectTag(Guid guid) : base(guid)
        {
        }

        /// <inheritdoc />
        internal override Type GetReferenceType()
        {
            return typeof(T);
        }
    }
}
