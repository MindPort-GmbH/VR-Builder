using System;
using UnityEngine.Events;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.BasicInteraction.Properties
{
    /// <summary>
    /// A control that can be triggered before returning to its default state, e.g. a button.
    /// </summary>
    public interface IMomentaryControlProperty : ISceneObjectProperty, ILockable
    {
        /// <summary>
        /// Called when the control is triggered.
        /// </summary>
        UnityEvent<MomentaryControlPropertyEventArgs> ControlTriggered { get; }

        /// <summary>
        /// Called when interaction with the control stops.
        /// </summary>
        UnityEvent<MomentaryControlPropertyEventArgs> ControlReleased { get; }

        /// <summary>
        /// Initializes the property so it is ready to be checked.
        /// </summary>
        void Initialize();

        /// <summary>
        /// True when the control has been triggered.
        /// </summary>
        bool HasBeenTriggered { get; }

        /// <summary>
        /// True when the control has been released.
        /// </summary>
        bool HasBeenReleased { get; }

        /// <summary>
        /// Simulates triggering the control.
        /// </summary>
        void FastForwardTrigger();
    }

    /// <summary>
    /// Event args for <see cref="IMomentaryControlProperty"/> events.
    /// </summary>
    public class MomentaryControlPropertyEventArgs : EventArgs
    {
    }
}