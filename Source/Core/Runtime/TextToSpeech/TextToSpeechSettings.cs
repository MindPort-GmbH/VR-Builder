using System.IO;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Settings;
using VRBuilder.Core.TextToSpeech.Configuration;

namespace VRBuilder.Core.TextToSpeech
{
    public class TextToSpeechSettings : SettingsObject<TextToSpeechSettings>
    {
        /// <summary>
        /// Name of the <see cref="ITextToSpeechProvider"/>.
        /// </summary>
        [HideInInspector]
        public string Provider;

        /// <summary>
        /// If true, the audio will not be generated at the building process
        /// </summary>
        public bool GenerateAudioInBuildingProcess = true;
        
        /// <summary>
        /// StreamingAsset directory name which is used to load/save audio files.
        /// </summary>
        public string StreamingAssetCacheDirectoryName = "TextToSpeech";

        /// <summary>
        /// SettingsObject for the tts settings
        /// </summary>
        public TextToSpeechSettings()
        {
            Provider = "MicrosoftSapiTextToSpeechProvider";
        }
    }
}