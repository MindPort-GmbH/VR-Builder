using UnityEngine.Events;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Base interface for a property that stores a single value.
    /// </summary>
    public interface IDataPropertyBase : ISceneObjectProperty
    {
        /// <summary>
        /// Raised when the stored value is reset to the default.
        /// </summary>
        UnityEvent OnValueReset { get; }

        /// <summary>
        /// Resets the value to its default.
        /// </summary>
        void ResetValue();
    }
}
