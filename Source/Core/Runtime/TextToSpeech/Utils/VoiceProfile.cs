namespace Source.Core.Runtime.TextToSpeech.Utils
{
    using System;
    using UnityEngine;

    namespace VRBuilder.Core.TextToSpeech
    {
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
            /// Voice ID specific to the TTS provider.
            /// </summary>
            [SerializeField]
            private string voiceId;

            /// <summary>
            /// Name of the TTS provider this profile is for.
            /// </summary>
            [SerializeField]
            private string providerName;

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

            public string VoiceId
            {
                get => voiceId;
                set => voiceId = value;
            }

            public string ProviderName
            {
                get => providerName;
                set => providerName = value;
            }

            public VoiceProfile()
            {
                displayName = "New Profile";
                languageCode = new []{"en-US"};
                voiceId = "";
                providerName = "";
            }

            public VoiceProfile(string displayName, string[] languageCode, string voiceId, string providerName)
            {
                this.displayName = displayName;
                this.languageCode = languageCode;
                this.voiceId = voiceId;
                this.providerName = providerName;
            }
        }
    }
}