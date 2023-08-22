using System;
using UnityEngine.Events;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Property that controls a particle system.
    /// </summary>
    public interface IParticleSystemProperty : ISceneObjectProperty
    {
        /// <summary>
        /// Called when the system starts emitting particles.
        /// </summary>
        UnityEvent<ParticleSystemPropertyEventArgs> StartedEmission { get; }

        /// <summary>
        /// Called when the system stops emitting particles.
        /// </summary>
        UnityEvent<ParticleSystemPropertyEventArgs> StoppedEmission { get; }

        /// <summary>
        /// True if the system is emitting particles.
        /// </summary>
        bool IsEmitting { get; }

        /// <summary>
        /// Start emitting particles.
        /// </summary>
        void StartEmission();

        /// <summary>
        /// Stop emitting particles.
        /// </summary>
        void StopEmission();
    }

    /// <summary>
    /// Event args for <see cref="IParticleSystemProperty"/>
    /// </summary>
    public class ParticleSystemPropertyEventArgs : EventArgs
    {
    }
}