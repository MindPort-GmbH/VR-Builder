using System;
using System.Runtime.Serialization;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Step inspector reference to a scene object or a specific property.
    /// </summary>    
    [DataContract(IsReference = true)]
    public abstract class ProcessSceneReference<T> : ProcessSceneReferenceBase where T : class
    {
        /// <inheritdoc />
        internal override Type GetReferenceType()
        {
            return typeof(T);
        }
    }
}
