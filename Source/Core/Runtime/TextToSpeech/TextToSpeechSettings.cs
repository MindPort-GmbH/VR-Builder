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
        /// Invoked when the text-to-speech provider changes.
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
        /// StreamingAsset directory name which is used to load/save audio files.
        /// </summary>
        [SerializeField]
        public string StreamingAssetCacheDirectoryName = "TextToSpeech";
        
        /// <summary>
        /// If true, the audio will not be generated at the building process.
        /// </summary>
        [SerializeField]
        public bool GenerateAudioInBuildingProcess = true;

        /// <summary>
        /// Displays more settings on the text-to-speech-behavior related behavior.
        /// </summary>
        [SerializeField]
        public bool ExtendedAudioSettingsActive = false;
        
        /// <summary>
        /// If true, the existing audio files for text-to-speech generation would be ignored.
        /// </summary>
        [SerializeField]
        public bool IgnoreExistingTextToSpeechFiles = false;
        
        /// <summary>
        /// Current active used audio type to generate text-to-speech files.
        /// </summary>
        [SerializeField]
        public SupportedAudioType SelectedAudioType = SupportedAudioType.WAV;
        
        /// <summary>
        /// Property for <see cref="voiceProfiles"/> which also calls <see cref="VoiceProfilesChanged"/> event.
        /// </summary>
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
        /// List of voice profiles for TTS providers.
        /// </summary>
        [SerializeField]
        private VoiceProfile[] voiceProfiles = Array.Empty<VoiceProfile>();
        
        [SerializeField]
        private string provider;

        private ITextToSpeechProvider currentProvider;

        /// <summary>
        /// SettingsObject for the tts settings.
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
        /// Returns the stringified enum in lowercase back.
        /// </summary>
        /// <param name="selectedAudioType">SupportedAudioType that should be used to string.</param>
        /// <returns>Returns the enum name as a string in lowercase.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Used not included enum element.</exception>
        public static string GetFileTypeName(SupportedAudioType selectedAudioType) => selectedAudioType switch
        {
            SupportedAudioType.WAV => "wav",
            SupportedAudioType.MP3 => "mp3",
            SupportedAudioType.OGG => "ogg",
            _ => throw new ArgumentOutOfRangeException(nameof(SupportedAudioType), $"Not expected direction value: {selectedAudioType}"),
        };
        
        /// <summary>
        /// Loads or sets the current TextToSpeechProvider.
        /// </summary>
        /// <returns>Returns the current set TextToSpeech provider over the <see cref="TextToSpeechProviderFactory"/>.</returns>
        public ITextToSpeechProvider GetCurrentTextToSpeechProvider()
        {
            return currentProvider ??= TextToSpeechProviderFactory.Instance.CreateProvider();
        }
        
        /// <summary>
        /// Gets the voice ID for a specific language and profile, trying all configured providers in order.
        /// </summary>
        public string GetVoiceId(string profileName, string languageCode, string providerName)
        {
            // Try to find the profile by name
            // If not found, try the default profile
            var profile = voiceProfiles.Where(p => p.DisplayName == profileName).ToArray();

            // If still no profile, use the default
            if (profile.Length == 0)
            {
                profile = voiceProfiles.Where(p => p.DisplayName == "Default").ToArray();
            }

            foreach (var profiles in profile)
            {
                // Check if the profile supports the language and provider
                if (profiles.LanguageCode.Contains(languageCode) || profiles.LanguageCode.Contains("all"))
                {
                    var mapping = profiles.ProviderVoiceMappings.FirstOrDefault(m => m.ProviderName == providerName);
                    if (mapping != null)
                    {
                        return mapping.VoiceId;
                    }
                }
            }
            
            // Fallback: Search for any profile that matches the language and provider
            return GetVoiceIdForLanguage(languageCode, providerName);
        }

        /// <summary>
        /// Gets the voice ID for a specific language, trying all configured providers in order.
        /// </summary>
        public string GetVoiceIdForLanguage(string languageCode, string providerName)
        {
            // Try to find a profile that matches both language and provider
            var profile = voiceProfiles.FirstOrDefault(p =>
                (p.LanguageCode.Contains(languageCode) || p.LanguageCode.Contains("all")) &&
                p.ProviderVoiceMappings.Any(m => m.ProviderName == providerName));

            if (profile != null)
            {
                return profile.ProviderVoiceMappings.First(m => m.ProviderName == providerName).VoiceId;
            }

            Debug.LogWarning($"No voice ID for language {languageCode} and provider {providerName} found. Using other profiles that contains the provider.");
            
            // If no exact match, try profiles that include the provider in their list
            profile = voiceProfiles.FirstOrDefault(p =>
                (p.LanguageCode.Contains(languageCode) || p.LanguageCode.Contains("all")) &&
                p.ProviderVoiceMappings.Count > 0);

            return profile != null ? profile.ProviderVoiceMappings[0].VoiceId : string.Empty;
        }

        /// <summary>
        /// Gets all profiles for a specific provider or providers that include it.
        /// </summary>
        public VoiceProfile[] GetProfilesForProvider(string providerName)
        {
            return voiceProfiles.Where(p =>
                p.ProviderVoiceMappings.Any(m => m.ProviderName == providerName) ||
                p.ProviderVoiceMappings.Count == 0).ToArray();
        }
        
        /// <summary>
        /// Triggers the <see cref="VoiceProfilesChanged"/> event.
        /// </summary>
        public void TriggerVoiceProfilesChanged()
        {
            VoiceProfilesChanged?.Invoke();
        }
        
        /// <summary>
        /// Supported audio file types which can be used for generate TTS-files by the specific provider.
        /// </summary>
        public enum SupportedAudioType
        {
            /// <summary>
            /// Default type that works on every platform
            /// </summary>
            WAV,
            MP3,
            OGG
        }
    }
}