using UnityEngine;
using UnityEngine.Events;
using VRBuilder.Core.Utils.Audio;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Implementation of the <see cref="IAudioSourceProperty"/> class.
    /// </summary>
    public class AudioSourceProperty: ProcessSceneObjectProperty, IAudioSourceProperty
    {
        /// <inheritdoc/>
        [Header("Events")]
        public UnityEvent<(IAudioData, string)> PlayTextToSpeech
        {
            get => playTextToSpeech;
        }
        
        /// <inheritdoc/>
        public UnityEvent<(IAudioData, string)> EndTextToSpeech
        {
            get => endTextToSpeech;
        }

        /// <inheritdoc/>
        public AudioSource AudioPlayer
        {
            get
            {
                return audioSource ??= GetComponent<AudioSource>();
            }
        }

        /// <inheritdoc/>
        public void OnPlayTextToSpeechPlay(IAudioData audioData, string subtitleKey)
        {
            
            PlayTextToSpeech.Invoke((audioData, subtitleKey));
        }

        /// <inheritdoc/>
        public void OnEndTextToSpeechPlay(IAudioData audioData, string subtitleKey)
        {
            EndTextToSpeech.Invoke((audioData, subtitleKey));
        }

        [Header("Settings")]
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private UnityEvent<(IAudioData, string)> playTextToSpeech = new();

        [SerializeField]
        private UnityEvent<(IAudioData, string)> endTextToSpeech = new();
    }
}