using System;
using UnityEngine.Events;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.BasicInteraction.Properties
{
    /// <summary>
    /// Property for objects that can be poked.
    /// Exposes live poke depth from XRPokeFilter and start/end events.
    /// Does not require a Rigidbody.
    /// </summary>
    public interface IPokableProperty : ISceneObjectProperty, ILockable
    {
        /// <summary>
        /// Called when the object starts being poked (any contact).
        /// </summary>
        UnityEvent<PokablePropertyEventArgs> PokeStarted { get; }

        /// <summary>
        /// Called when the poke ends.
        /// </summary>
        UnityEvent<PokablePropertyEventArgs> PokeEnded { get; }

        /// <summary>
        /// Is the object currently being poked (any contact).
        /// </summary>
        bool IsBeingPoked { get; }

        /// <summary>
        /// Current poke depth from 0 (no contact) to 1 (fully pressed).
        /// Updated every frame from XRPokeFilter's pokeStateData.
        /// </summary>
        float CurrentPokeDepth { get; }

        /// <summary>
        /// Instantaneously simulate that the object was poked.
        /// </summary>
        void FastForwardPoke();

        /// <summary>
        /// Force this property to a specified poked state with an explicit depth.
        /// </summary>
        void ForceSetPokeState(bool isPoked, float depth);
    }

    /// <summary>
    /// Event args for IPokableProperty events.
    /// </summary>
    public class PokablePropertyEventArgs : EventArgs
    {
    }
}
