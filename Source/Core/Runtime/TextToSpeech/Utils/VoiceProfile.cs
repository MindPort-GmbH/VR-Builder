namespace Source.Core.Runtime.TextToSpeech.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    namespace VRBuilder.Core.TextToSpeech
    {
        [Serializable]
        public class ProviderVoiceMapping
        {
            public string ProviderName;
            public string VoiceId;

            public ProviderVoiceMapping(string providerName, string voiceId)
            {
                ProviderName = providerName;
                VoiceId = voiceId;
            }
        }

        /// <summary>
        /// Represents a voice profile that maps a display name to a specific voice ID for a TTS provider and language.
        /// </summary>
        [Serializable]
        public class VoiceProfile
        {
            /// <summary>
            /// Display name of the profile (e.g., "Marcello", "Markus").
            /// </summary>
            [SerializeField]
            private string displayName;

            /// <summary>
            /// ISO language code (e.g., "de-DE", "en-US").
            /// </summary>
            [SerializeField]
            private string[] languageCode;

            /// <summary>
            /// Mappings from provider names to voice IDs.
            /// </summary>
            [SerializeField]
            private List<ProviderVoiceMapping> providerVoiceMappings;
            
            /// <summary>
            /// Fallback provider if there is no avaibled provider for multiple voices
            /// </summary>
            private string fallbackProviderName;

            public string DisplayName
            {
                get => displayName;
                set => displayName = value;
            }

            public string[] LanguageCode
            {
                get => languageCode;
                set => languageCode = value;
            }

            public List<ProviderVoiceMapping> ProviderVoiceMappings
            {
                get => providerVoiceMappings;
                set => providerVoiceMappings = value;
            }

            public string FallbackProviderName
            {
                get => fallbackProviderName; 
                set => fallbackProviderName = value;
            }

            public VoiceProfile()
            {
                displayName = "New Profile";
                languageCode = new []{"all"};
                providerVoiceMappings = new List<ProviderVoiceMapping>();
            }

            public VoiceProfile(string displayName, string[] languageCode, string voiceId, string[] providerNames)
            {
                this.displayName = displayName;
                this.languageCode = languageCode;
                this.providerVoiceMappings = providerNames.Select(p => new ProviderVoiceMapping(p, voiceId)).ToList();
            }
        }
    }
}