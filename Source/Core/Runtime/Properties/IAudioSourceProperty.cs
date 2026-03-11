using UnityEngine;
using UnityEngine.Events;
using VRBuilder.Core.Utils.Audio;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Property for the connection of a scene-based audio player.
    /// </summary>
    public interface IAudioSourceProperty : ISceneObjectProperty
    {
        /// <summary>
        /// Called when a new text to speech is played.
        /// </summary>
        public UnityEvent<(IAudioData, string)> PlayTextToSpeech { get; }

        /// <summary>
        /// Called when a new text to speech is played.
        /// </summary>
        public UnityEvent<(IAudioData, string)> EndTextToSpeech { get; }
        
        /// <summary>
        /// Get the selected audio player.
        /// </summary>
        public AudioSource AudioPlayer { get; }

        void OnPlayTextToSpeechPlay(IAudioData dataAudioData, string dataSubtitleKey);
        
        void OnEndTextToSpeechPlay(IAudioData dataAudioData, string dataSubtitleKey);
    }
}