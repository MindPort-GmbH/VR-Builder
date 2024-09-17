using System;
using System.Runtime.Serialization;
using Source.TextToSpeech_Component.Runtime;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Audio;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Localization;

namespace VRBuilder.TextToSpeech.Audio
{
    /// <summary>
    /// This class retrieves and stores AudioClips generated based in a provided localized text. 
    /// </summary>
    [DataContract(IsReference = true)]
    [Core.Attributes.DisplayName("Play Text to Speech")]
    public class TextToSpeechAudio : TextToSpeechContent, IAudioData
    {
        private bool isLoading;
        private string text;

        /// <inheritdoc/>
        [DataMember]
        [UsesSpecificProcessDrawer("MultiLineStringDrawer")]
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
                ITextToSpeechConfiguration ttsConfiguration = RuntimeConfigurator.Configuration.GetTextToSpeechConfiguration();
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
