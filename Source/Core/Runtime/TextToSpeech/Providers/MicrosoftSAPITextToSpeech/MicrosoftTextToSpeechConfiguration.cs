using UnityEngine;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.TextToSpeech.Configuration
{
    public class MicrosoftTextToSpeechConfiguration : SettingsObject<MicrosoftTextToSpeechConfiguration>, ITextToSpeechConfiguration
    {
        /// <summary>
        /// Voice that should be used.
        /// </summary>
        [SerializeField]
        private string voice = "Male";

        /// <summary>
        /// Property of <see cref="voice"/>
        /// </summary>
        public string Voice => voice;
    }
}