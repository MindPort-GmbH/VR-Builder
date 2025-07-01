using System;
using UnityEngine.Events;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.BasicInteraction.Properties
{
    /// <summary>
    /// A control that can be set to a value, e.g. a dial or a switch.
    /// </summary>
    /// <typeparam name="T">The type of value the control can be set to.</typeparam>
    public interface ISettableControlProperty<T> : ISceneObjectProperty, ILockable
    {
        /// <summary>
        /// Called when interaction with the control starts.
        /// </summary>
        UnityEvent<SettableControlPropertyEventArgs<T>> InteractionStarted { get; }

        /// <summary>
        /// Called when interaction with the control ends.
        /// </summary>
        UnityEvent<SettableControlPropertyEventArgs<T>> InteractionEnded { get; }

        /// <summary>
        /// Called when the value of the control changes.
        /// </summary>
        UnityEvent<T> ValueChanged { get; }

        /// <summary>
        /// True if a user is interacting with the control.
        /// </summary>
        bool IsInteracting { get; }

        /// <summary>
        /// Current value of the control.
        /// </summary>
        T CurrentValue { get; }

        /// <summary>
        /// Simulates setting the control to the specified value.
        /// </summary>        
        void FastForwardValue(T value);
    }

    /// <summary>
    /// Event args for <see cref="ISettableControlProperty{T}"/> events.
    /// </summary>
    public class SettableControlPropertyEventArgs<T> : EventArgs
    {
    }
}