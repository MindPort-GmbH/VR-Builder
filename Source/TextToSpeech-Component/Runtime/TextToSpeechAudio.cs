using System;
using UnityEngine;
using System.Runtime.Serialization;
using VRBuilder.Core.Audio;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Configuration;

namespace VRBuilder.TextToSpeech.Audio
{
    /// <summary>
    /// This class retrieves and stores AudioClips generated based in a provided localized text. 
    /// </summary>
    [DataContract(IsReference = true)]
    [DisplayName("Play Text to Speech")]
    public class TextToSpeechAudio : IAudioData
    {
        private bool isLoading;
        private string text;

        [DataMember]
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                InitializeAudioClip();
            }
        }

        protected TextToSpeechAudio()
        {
            text = "";
        }

        public TextToSpeechAudio(string text)
        {
            Text = text;
        }

        /// <summary>
        /// True when there is an Audio Clip loaded.
        /// </summary>
        public bool HasAudioClip
        {
            get
            {
                return AudioClip != null;
            }
        }

        /// <summary>
        /// Returns true only when is busy loading an Audio Clip.
        /// </summary>
        public bool IsLoading
        {
            get { return isLoading; }
        }

        public AudioClip AudioClip { get; private set; }

        private async void InitializeAudioClip()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (Text == null)
            {
                Debug.LogWarning("No text provided");
                return;
            }

            if (string.IsNullOrEmpty(Text))
            {
                Debug.LogWarning($"No text provided.");
                return;
            }

            isLoading = true;
            
            try
            {
                TextToSpeechConfiguration ttsConfiguration = RuntimeConfigurator.Configuration.GetTextToSpeechConfiguration();
                ITextToSpeechProvider provider = TextToSpeechProviderFactory.Instance.CreateProvider(ttsConfiguration);

                AudioClip = await provider.ConvertTextToSpeech(Text);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(exception.Message);
            }
            
            isLoading = false;
        }

        /// <inheritdoc/>
        public bool IsEmpty()
        {
            return Text == null || (string.IsNullOrEmpty(Text));
        }
    }
}
