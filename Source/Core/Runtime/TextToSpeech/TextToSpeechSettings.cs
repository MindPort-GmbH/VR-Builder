using System;
using System.Linq;
using Source.Core.Runtime.TextToSpeech.Utils.VRBuilder.Core.TextToSpeech;
using UnityEngine;
using VRBuilder.Core.Settings;
using VRBuilder.Core.TextToSpeech.Providers;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.TextToSpeech
{
    public class TextToSpeechSettings : SettingsObject<TextToSpeechSettings>
    {
        /// <summary>
        /// Invoked when the text-to-speech provider changes
        /// </summary>
        public event Action ProviderChanged;

        /// <summary>
        /// Name of the <see cref="ITextToSpeechProvider"/>.
        /// </summary>
        [HideInInspector]
        public string Provider
        {
            get => provider;
            set
            {
                provider = value;
                if (currentProvider?.GetType().Name != Provider)
                {
                    currentProvider = null;
                    ProviderChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// If true, the audio will not be generated at the building process
        /// </summary>
        public bool GenerateAudioInBuildingProcess = true;
        
        /// <summary>
        /// StreamingAsset directory name which is used to load/save audio files.
        /// </summary>
        public string StreamingAssetCacheDirectoryName = "TextToSpeech";

        /// <summary>
        /// List of voice profiles for TTS providers.
        /// </summary>
        [SerializeField]
        private VoiceProfile[] voiceProfiles = Array.Empty<VoiceProfile>();

        public VoiceProfile[] VoiceProfiles
        {
            get => voiceProfiles;
            set => voiceProfiles = value;
        }
        
        [SerializeField]
        private string provider;

        private ITextToSpeechProvider currentProvider;
        
        /// <summary>
        /// SettingsObject for the tts settings
        /// </summary>
        public TextToSpeechSettings()
        {
            Provider = "MicrosoftSapiTextToSpeechProvider";
        }
        
        /// <summary>
        /// Loads the current TextToSpeechProvider
        /// </summary>
        /// <returns></returns>
        public ITextToSpeechProvider GetCurrentTextToSpeechProvider()
        {
            return currentProvider ??= TextToSpeechProviderFactory.Instance.CreateProvider();
        }
        
        /// <summary>
        /// Gets the voice ID for a specific language, trying all configured providers in order
        /// </summary>
        public string GetVoiceIdForLanguage(string languageCode, string providerName)
        {
            // First try to find a profile that matches both language and provider
            var profile = voiceProfiles.FirstOrDefault(p =>
                p.LanguageCode.Contains(languageCode) &&
                p.ProviderNames.Contains(providerName));

            if (profile != null)
                return profile.VoiceId;

            // If no exact match, try profiles that include the provider in their list
            profile = voiceProfiles.FirstOrDefault(p =>
                p.LanguageCode.Contains(languageCode) &&
                p.ProviderNames.Length > 0);

            if (profile != null)
                return profile.VoiceId;

            // If no match with language, try fallback provider
            if (!string.IsNullOrEmpty(currentProvider.GetType().Name))
            {
                profile = voiceProfiles.FirstOrDefault(p =>
                    p.LanguageCode.Contains(languageCode) &&
                    p.ProviderNames.Contains(currentProvider.GetType().Name));

                if (profile != null)
                    return profile.VoiceId;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets all profiles for a specific provider or providers that include it
        /// </summary>
        public VoiceProfile[] GetProfilesForProvider(string providerName)
        {
            return voiceProfiles.Where(p =>
                p.ProviderNames.Contains(providerName) ||
                p.ProviderNames.Length == 0).ToArray();
        }
    }
}