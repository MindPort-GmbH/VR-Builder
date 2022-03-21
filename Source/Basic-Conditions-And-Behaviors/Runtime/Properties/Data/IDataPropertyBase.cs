using System;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Base interface for a property that stores a single value.
    /// </summary>
    public interface IDataPropertyBase : ISceneObjectProperty
    {
        /// <summary>
        /// Raised when the stored value changes.
        /// </summary>
        event EventHandler<EventArgs> ValueChanged;

        /// <summary>
        /// Raised when the stored value is reset to the default.
        /// </summary>
        event EventHandler<EventArgs> ValueReset;

        /// <summary>
        /// Resets the value to its default.
        /// </summary>
        void ResetValue();
    }
}
