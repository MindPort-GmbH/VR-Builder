using System;
using System.Linq;
using Source.TextToSpeech;
using UnityEngine;
using VRBuilder.Core.Settings;
using VRBuilder.Core.TextToSpeech.Providers;
using static Source.TextToSpeech.ITextToSpeechConfiguration;

namespace VRBuilder.Core.TextToSpeech
{
    public class TextToSpeechProviderSettings : SettingsObject<TextToSpeechProviderSettings>, ITextToSpeechConfiguration
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
        /// Current active used audio type to generate text-to-speech files.
        /// </summary>
        public SupportedAudioType SelectedAudioType
        {
            get => selectedAudioType;
            set => selectedAudioType = value;
        }

        public string StreamingAssetCacheDirectoryName
        {
            get => streamingAssetCacheDirectoryName;
            set => streamingAssetCacheDirectoryName = value;
        }

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
        /// Current active used audio type to generate text-to-speech files.
        /// </summary>
        [SerializeField]
        private SupportedAudioType selectedAudioType = SupportedAudioType.WAV;

        [SerializeField]
        private string streamingAssetCacheDirectoryName = "TextToSpeech";

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
        public TextToSpeechProviderSettings()
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
        /// Gets the voice ID for a specific language and profile, trying all configured providers in order.
        /// </summary>
        public string GetVoiceId(string profileName, string languageCode, string providerName)
        {
            // Try to find the profile by name
            // If not found, try the default profile
            VoiceProfile[] profile = voiceProfiles.Where(p => p.DisplayName == profileName).ToArray();

            // If still no profile, use the default
            if (profile.Length == 0)
            {
                profile = voiceProfiles.Where(p => p.DisplayName == "Default").ToArray();
            }

            foreach (VoiceProfile profiles in profile)
            {
                // Check if the profile supports the language and provider
                if (profiles.LanguageCode.Contains(languageCode) || profiles.LanguageCode.Contains("all"))
                {
                    ProviderVoiceMapping mapping = profiles.ProviderVoiceMappings.FirstOrDefault(m => m.ProviderName == providerName);
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
            VoiceProfile profile = voiceProfiles.FirstOrDefault(p =>
                (p.LanguageCode.Contains(languageCode) || p.LanguageCode.Contains("all")) &&
                p.ProviderVoiceMappings.Any(m => m.ProviderName == providerName));

            if (profile != null)
            {
                return profile.ProviderVoiceMappings.First(m => m.ProviderName == providerName).VoiceId;
            }

            ForwardingLogger.LogWarning($"No voice ID for language {languageCode} and provider {providerName} found. Using other profiles that contains the provider.");

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
    }
}
