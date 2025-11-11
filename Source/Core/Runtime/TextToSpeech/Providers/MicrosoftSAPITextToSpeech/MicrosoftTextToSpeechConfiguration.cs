using UnityEngine;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.TextToSpeech.Configuration
{
    public class MicrosoftTextToSpeechConfiguration : SettingsObject<MicrosoftTextToSpeechConfiguration>, ITextToSpeechConfiguration
    {
        /// <summary>
        /// Voice that should be used for text-to-speech generation. The available voices are based on the selected language.
        /// </summary>
        [SerializeField]
        private string voice = "Male";

        /// <summary>
        /// Voice that should be used for text-to-speech generation. The available voices are based on the selected language.
        /// </summary>
        public string Voice => voice;
    }
}