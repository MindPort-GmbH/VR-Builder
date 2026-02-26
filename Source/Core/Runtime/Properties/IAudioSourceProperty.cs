using UnityEngine;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Property for the connection of a scene-based audio player.
    /// </summary>
    public interface IAudioSourceProperty : ISceneObjectProperty
    {
        /// <summary>
        /// Get the selected audio player.
        /// </summary>
        public AudioSource AudioPlayer { get; }
    }
}