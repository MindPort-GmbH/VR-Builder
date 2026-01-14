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
        /// Gets the voice ID for a specific language from the configured profiles for the current provider.
        /// </summary>
        /// <param name="languageCode">ISO language code</param>
        /// <param name="providerName">Name of the TTS provider</param>
        /// <returns>Voice ID or empty string if not found</returns>
        public string GetVoiceIdForLanguage(string languageCode, string providerName)
        {
            var profile = voiceProfiles.FirstOrDefault(p => 
                p.LanguageCode.Contains(languageCode) && 
                p.ProviderName == providerName);
            
            return profile?.VoiceId ?? string.Empty;
        }

        /// <summary>
        /// Gets all profiles for a specific provider.
        /// </summary>
        /// <param name="providerName">Name of the TTS provider</param>
        /// <returns>Array of voice profiles for the provider</returns>
        public VoiceProfile[] GetProfilesForProvider(string providerName)
        {
            return voiceProfiles.Where(p => p.ProviderName == providerName).ToArray();
        }
    }
}