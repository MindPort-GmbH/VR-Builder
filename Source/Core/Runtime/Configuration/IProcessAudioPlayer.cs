using UnityEngine;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Interface for the class playing sounds for the process, i.e. tts and play audio behaviors.
    /// </summary>
    public interface IProcessAudioPlayer
    {
        /// <summary>
        /// Gets a fallback audio source. Used only for backwards compatibility.
        /// </summary>
        AudioSource FallbackAudioSource { get; }

        /// <summary>
        /// True if currently playing audio.
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Play the specified audio clip immediately with the set parameters.
        /// </summary>
        void PlayAudioClip(AudioClip audioClip, float volume = 1f, float pitch = 1f);

        /// <summary>
        /// Stops playing audio.
        /// </summary>
        void Stop();

        /// <summary>
        /// Resets the player to its default settings.
        /// </summary>
        void Reset();
    }
}
