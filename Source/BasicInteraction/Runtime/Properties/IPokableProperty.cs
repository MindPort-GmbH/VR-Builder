using System;
using UnityEngine.Events;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.BasicInteraction.Properties
{
    /// <summary>
    /// Property for objects that can be poked.
    /// Unlike touchable objects, pokable objects use XRSimpleInteractable and do not require a Rigidbody.
    /// Pokable objects cannot be grabbed or used.
    /// </summary>
    public interface IPokableProperty : ISceneObjectProperty, ILockable
    {
        /// <summary>
        /// Called when the object is poked.
        /// </summary>
        UnityEvent<PokablePropertyEventArgs> PokeStarted { get; }

        /// <summary>
        /// Called when the poke ends.
        /// </summary>
        UnityEvent<PokablePropertyEventArgs> PokeEnded { get; }

        /// <summary>
        /// Is the object currently being poked.
        /// </summary>
        bool IsBeingPoked { get; }

        /// <summary>
        /// Instantaneously simulate that the object was poked.
        /// </summary>
        void FastForwardPoke();

        /// <summary>
        /// Force this property to a specified poked state.
        /// </summary>
        void ForceSetPoked(bool isPoked);
    }

    /// <summary>
    /// Event args for IPokableProperty events.
    /// </summary>
    public class PokablePropertyEventArgs : EventArgs
    {
    }
}
