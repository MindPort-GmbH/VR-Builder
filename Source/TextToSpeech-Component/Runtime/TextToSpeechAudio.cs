using System;
using UnityEngine;
using System.Runtime.Serialization;
using VRBuilder.Core.Audio;
using VRBuilder.Core.Configuration;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using VRBuilder.Core.Localization;

namespace VRBuilder.TextToSpeech.Audio
{
    /// <summary>
    /// This class retrieves and stores AudioClips generated based in a provided localized text. 
    /// </summary>
    [DataContract(IsReference = true)]
    [VRBuilder.Core.Attributes.DisplayName("Play Text to Speech")]
    public class TextToSpeechAudio : TextToSpeechContent, IAudioData
    {
        private bool isLoading;
        private string text;

        /// <inheritdoc/>
        [DataMember]
        [Core.Attributes.DisplayName("Text/Key")]
        public override string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                if (Application.isPlaying)
                {
                    InitializeAudioClip();
                }
            }
        }

        protected TextToSpeechAudio() : this("")
        {
        }

        public TextToSpeechAudio(string text)
        {
            this.text = text;

            if (LocalizationSettings.HasSettings)
            {
                LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
            }
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

        /// <inheritdoc/>
        public string ClipData
        {
            get
            {
                return Text;
            }
            set
            {
                Text = value;
            }
        }

        /// <summary>
        /// Creates the audio clip based on the provided parameters.
        /// </summary>
        public async void InitializeAudioClip()
        {
            AudioClip = null;

            if (string.IsNullOrEmpty(Text))
            {
                Debug.LogWarning($"No text provided.");
                return;
            }

            isLoading = true;
            
            try
            {
                TextToSpeechConfiguration ttsConfiguration = RuntimeConfigurator.Configuration.GetTextToSpeechConfiguration();
                ITextToSpeechProvider provider = new FileTextToSpeechProvider(ttsConfiguration);
                AudioClip = await provider.ConvertTextToSpeech(GetLocalizedContent(), LanguageSettings.Instance.ActiveOrDefaultLocale);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(exception.Message);
            }
            
            isLoading = false;
        }

        private void OnSelectedLocaleChanged(Locale locale)
        {
            if (Application.isPlaying)
            {
                InitializeAudioClip();
            }
        }

        /// <inheritdoc/>
        public bool IsEmpty()
        {
            return Text == null || (string.IsNullOrEmpty(Text));
        }

        public override string GetLocalizedContent()
        {          
            return LanguageUtils.GetLocalizedString(Text, RuntimeConfigurator.Instance.GetProcessStringLocalizationTable(), LanguageSettings.Instance.ActiveOrDefaultLocale);
        }
    }
}
