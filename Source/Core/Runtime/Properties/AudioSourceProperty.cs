using System;
using Core.Runtime.Utils;
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
        [Header("Events")]
        [SerializeField]
        private UnityEvent<(IAudioData, string)> playTextToSpeech = new UnityEvent<(IAudioData, string)>();

        [SerializeField]
        private UnityEvent<(IAudioData, string)> endTextToSpeech = new UnityEvent<(IAudioData, string)>();

        [Header("Settings")]
        [SerializeField]
        private AudioSource audioSource;

        private Action<(IAudioData, string)> startedPlayTextToSpeech;
        private Action<(IAudioData, string)> stoppedTextToSpeech;

        event Action<(IAudioData, string)> IAudioSourceProperty.PlayTextToSpeech
        {
            add => startedPlayTextToSpeech += value;
            remove => startedPlayTextToSpeech += value;
        }

        event Action<(IAudioData, string)> IAudioSourceProperty.EndTextToSpeech
        {
            add => stoppedTextToSpeech += value;
            remove => stoppedTextToSpeech += value;
        }

        /// <inheritdoc/>
        public IAudioData AudioPlayer
        {
            get
            {
                return audioSource.ToAudioData();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            playTextToSpeech.AddListener(OnPlayTextToSpeechPlay);
            endTextToSpeech.AddListener(OnEndTextToSpeechPlay);
        }

        protected void OnDisable()
        {
            playTextToSpeech.RemoveListener(OnPlayTextToSpeechPlay);
            endTextToSpeech.RemoveListener(OnEndTextToSpeechPlay);
        }

        public void OnPlayTextToSpeechPlay((IAudioData audioData, string subtitleKey) data)
        {
            startedPlayTextToSpeech.Invoke(data);
        }

        public void OnEndTextToSpeechPlay((IAudioData audioData, string subtitleKey) data)
        {
            stoppedTextToSpeech.Invoke(data);
        }
    }
}
