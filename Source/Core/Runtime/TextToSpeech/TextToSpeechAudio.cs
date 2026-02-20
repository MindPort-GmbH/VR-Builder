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
using VRBuilder.Core.TextToSpeech.Providers;
using VRBuilder.Core.Utils.Audio;

namespace VRBuilder.Core.TextToSpeech
{
    /// <summary>
    /// This class retrieves and stores AudioClips generated based in a provided localized text. 
    /// </summary>
    [DataContract(IsReference = true)]
    [Attributes.DisplayName("Play Text to Speech")]
    public class TextToSpeechAudio : TextToSpeechContent, IAudioData
    {
        private bool isReady;
        private bool isLoading;
        private string text;
        private string speaker;
        private AudioClip audioClip;

        /// <inheritdoc/>
        [DataMember]
        [UsesSpecificProcessDrawer("MultiLineStringDrawer")]
        [Attributes.DisplayName("Text/Key")]
        public override string Text
        {
            get => text;
            set => text = value;
        }

        /// <inheritdoc/>
        [DataMember]
        [UsesSpecificProcessDrawer("SpeakerDropdownDrawer")]
        [Attributes.DisplayName("Selected Profile")]
        public override string Speaker
        {
            get => speaker;
            set => speaker = value;
        }
        
        protected TextToSpeechAudio() : this("")
        {
        }

        public TextToSpeechAudio(string text, string speaker = "")
        {
            this.text = text;
            this.speaker = speaker;

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
                if (IsEmpty())
                    return false;
                return audioClip != null;
            }
        }

        /// <inheritdoc/>
        bool IAudioData.IsReady => isReady;

        /// <inheritdoc/>
        bool IAudioData.IsLoading => isLoading;

        /// <inheritdoc/>
        public AudioClip AudioClip
        {
            get => audioClip;
            private set => audioClip = value;
        }

        /// <inheritdoc/>
        public string ClipData
        {
            get => Text;
            set => Text = value;
        }

        /// <summary>
        /// Creates the audio clip based on the provided parameters.
        /// </summary>
        public void InitializeAudioClip()
        {
#if UNITY_EDITOR
            //refresh the clip if the clip name changed
            if (isReady && AudioClip?.name != text)
            {
                AudioClip = null;
            }
#endif
            if (isReady && AudioClip)
            {
                return;
            }
            AudioClip = null;
            isReady = false;
            isLoading = true;

            if (IsEmpty())
            {
                Debug.LogWarning($"No text provided.");
                isLoading = false;
                return;
            }

            ITextToSpeechProvider provider = new FileTextToSpeechProvider();

            string usedKey = "";
            string usedText;

            if (RuntimeConfigurator.Instance.GetProcessStringLocalizationTable() != "")
            {
                usedKey = text;
                usedText = GetLocalizedContent();
            }
            else
            {
                usedText = text;
            }

            Task<AudioClip> t = provider.ConvertTextToSpeech(usedKey, usedText, LanguageSettings.Instance.ActiveOrDefaultLocale, Speaker);
            t.ContinueWith(task =>
            {
                try
                {
                    AudioClip = task.Result;
                    isReady = true;
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(exception.Message);
                    isReady = false;
                }
                finally
                {
                    isLoading = false;
                }
            });
        }

        private void OnSelectedLocaleChanged(Locale locale)
        {
            if (Application.isPlaying && !IsEmpty())
            {
                InitializeAudioClip();
            }
        }

        /// <inheritdoc/>
        public bool IsEmpty()
        {
            return Text == null || string.IsNullOrEmpty(Text);
        }

        public override string GetLocalizedContent()
        {
            string processStringLocalizationTable = RuntimeConfigurator.Instance.GetProcessStringLocalizationTable();

            return LanguageUtils.GetLocalizedString(Text, processStringLocalizationTable, LanguageSettings.Instance.ActiveOrDefaultLocale);
        }
    }
}