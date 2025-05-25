using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Localization;
using VRBuilder.Core.Settings;
using VRBuilder.Core.TextToSpeech.Configuration;
using VRBuilder.Core.TextToSpeech.Providers;
using VRBuilder.Core.Utils.Audio;

namespace VRBuilder.Core.TextToSpeech
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
        private AudioClip audioClip;

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
                return !IsEmpty() && audioClip != null;
            }
        }
        
        /// <inheritdoc/>
        bool IAudioData.IsLoading
        {
            get
            {
                return isLoading;
            }
        }

        /// <inheritdoc/>
        public AudioClip AudioClip
        {
            get
            {
                if (audioClip == null)
                {
                    InitializeAudioClip();
                }
                return audioClip;
            }
            private set => audioClip = value;
        }

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
        public async Task InitializeAudioClip()
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
                await Task.CompletedTask;
            }
            catch (Exception exception)
            {
                Debug.LogWarning(exception.Message);
            }    
            finally
            {
                isLoading = false;
            }
        }

        private void OnSelectedLocaleChanged(Locale locale)
        {
            if (Application.isPlaying)
            {
                InitializeAudioClip().Start();
            }
        }

        /// <inheritdoc/>
        public bool IsEmpty()
        {
            return Text == null || string.IsNullOrEmpty(Text);
        }

        public override string GetLocalizedContent()
        {
            return LanguageUtils.GetLocalizedString(Text, RuntimeConfigurator.Instance.GetProcessStringLocalizationTable(), LanguageSettings.Instance.ActiveOrDefaultLocale);
        }
    }
}
