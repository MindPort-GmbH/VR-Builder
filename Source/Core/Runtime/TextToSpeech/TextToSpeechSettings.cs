using System;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.Settings;
using VRBuilder.Core.TextToSpeech.Providers;

namespace VRBuilder.Core.TextToSpeech
{
    public class TextToSpeechSettings : SettingsObject<TextToSpeechSettings>
    {
        /// <summary>
        /// Invoked when the text-to-speech provider changes
        /// </summary>
        public event Action ProviderChanged;

        /// <summary>
        /// Invoked when the voice profiles are changed.
        /// </summary>
        public event Action VoiceProfilesChanged;

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
        [SerializeField]
        public bool GenerateAudioInBuildingProcess = true;
        
        /// <summary>
        /// StreamingAsset directory name which is used to load/save audio files.
        /// </summary>
        [SerializeField]
        public string StreamingAssetCacheDirectoryName = "TextToSpeech";

        /// <summary>
        /// List of voice profiles for TTS providers.
        /// </summary>
        [SerializeField]
        private VoiceProfile[] voiceProfiles = Array.Empty<VoiceProfile>();

        public VoiceProfile[] VoiceProfiles
        {
            get => voiceProfiles;
            set
            {
                voiceProfiles = value;
                TriggerVoiceProfilesChanged();
            }
        }

        /// <summary>
        /// Triggers the <see cref="VoiceProfilesChanged"/> event.
        /// </summary>
        public void TriggerVoiceProfilesChanged()
        {
            VoiceProfilesChanged?.Invoke();
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
            if (voiceProfiles.Length == 0)
            {
                voiceProfiles = new[] { new VoiceProfile("Default", new[] { "all" }, "None Voice Selectable", new[] { "MicrosoftSapiTextToSpeechProvider" }) };
            }
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
        /// Gets the voice ID for a specific language and profile, trying all configured providers in order
        /// </summary>
        public string GetVoiceId(string profileName, string languageCode, string providerName)
        {
            // First try to find the profile by name
            var profile = voiceProfiles.FirstOrDefault(p => p.DisplayName == profileName);

            // If not found, try the default profile
            if (profile == null)
            {
                profile = voiceProfiles.FirstOrDefault(p => p.DisplayName == "Default");
            }

            // If still no profile, use the default behavior (empty voice ID)
            if (profile == null)
            {
                return string.Empty;
            }

            // Check if the profile supports the language and provider
            if (profile.LanguageCode.Contains(languageCode) || profile.LanguageCode.Contains("all"))
            {
                var mapping = profile.ProviderVoiceMappings.FirstOrDefault(m => m.ProviderName == providerName);
                if (mapping != null)
                {
                    return mapping.VoiceId;
                }
            }

            // Fallback: search for any profile that matches language and provider
            return GetVoiceIdForLanguage(languageCode, providerName);
        }

        /// <summary>
        /// Gets the voice ID for a specific language, trying all configured providers in order
        /// </summary>
        public string GetVoiceIdForLanguage(string languageCode, string providerName)
        {
            // First try to find a profile that matches both language and provider
            var profile = voiceProfiles.FirstOrDefault(p =>
                (p.LanguageCode.Contains(languageCode) || p.LanguageCode.Contains("all")) &&
                p.ProviderVoiceMappings.Any(m => m.ProviderName == providerName));

            if (profile != null)
            {
                return profile.ProviderVoiceMappings.First(m => m.ProviderName == providerName).VoiceId;
            }

            // If no exact match, try profiles that include the provider in their list
            profile = voiceProfiles.FirstOrDefault(p =>
                (p.LanguageCode.Contains(languageCode) || p.LanguageCode.Contains("all")) &&
                p.ProviderVoiceMappings.Count > 0);

            if (profile != null)
                return profile.ProviderVoiceMappings[0].VoiceId;

            return string.Empty;
        }

        /// <summary>
        /// Gets all profiles for a specific provider or providers that include it
        /// </summary>
        public VoiceProfile[] GetProfilesForProvider(string providerName)
        {
            return voiceProfiles.Where(p =>
                p.ProviderVoiceMappings.Any(m => m.ProviderName == providerName) ||
                p.ProviderVoiceMappings.Count == 0).ToArray();
        }
    }
}