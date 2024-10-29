using System.IO;
using UnityEngine;
using UnityEngine.Localization;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Settings;
using VRBuilder.Core.TextToSpeech.Utils;

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

        /// <summary>
        /// Loads the first existing <see cref="MicrosoftTextToSpeechConfiguration"/> found in the project.
        /// If any <see cref="MicrosoftTextToSpeechConfiguration"/> exist in the project it creates and saves a new instance with default values (editor only).
        /// </summary>
        /// <remarks>When used in runtime, this method can only retrieve config files located under a Resources folder.</remarks>
        public static MicrosoftTextToSpeechConfiguration LoadConfiguration()
        {
            MicrosoftTextToSpeechConfiguration configuration = Resources.Load<MicrosoftTextToSpeechConfiguration>(nameof(MicrosoftTextToSpeechConfiguration));
            return configuration != null ? configuration : CreateNewConfiguration();
        }

        /// <inheritdoc />
        public string GetUniqueIdentifier(string text, string md5Hash, Locale locale)
        {
            return $"TTS_{voice}_{locale.Identifier.Code}_{md5Hash.Replace("-", "")}";
        }

        /// <inheritdoc />
        public bool IsCached(Locale locale, string localizedContent)
        {
            string filename = this.GetUniqueTextToSpeechFilename(localizedContent, locale);
            string filePath = $"{RuntimeConfigurator.Configuration.GetTextToSpeechSettings().StreamingAssetCacheDirectoryName}/{filename}";
            return File.Exists(Path.Combine(Application.streamingAssetsPath, filePath));
        }

        private static MicrosoftTextToSpeechConfiguration CreateNewConfiguration()
        {
            MicrosoftTextToSpeechConfiguration microsoftTextToSpeechConfiguration = CreateInstance<MicrosoftTextToSpeechConfiguration>();
            RuntimeConfigurator.Configuration.SetTextToSpeechConfiguration(microsoftTextToSpeechConfiguration);

#if UNITY_EDITOR
            string resourcesPath = "Assets/MindPort/VR Builder/Resources/";
            string configFilePath = $"{resourcesPath}{nameof(MicrosoftTextToSpeechConfiguration)}.asset";

            if (Directory.Exists(resourcesPath) == false)
            {
                Directory.CreateDirectory(resourcesPath);
            }

            Debug.LogWarningFormat("No text to speech configuration found!\nA new configuration file was created at {0}", configFilePath);
            UnityEditor.AssetDatabase.CreateAsset(microsoftTextToSpeechConfiguration, configFilePath);
            UnityEditor.AssetDatabase.Refresh();

            if (Application.isPlaying == false)
            {
                UnityEditor.Selection.activeObject = microsoftTextToSpeechConfiguration;
            }
#endif

            return microsoftTextToSpeechConfiguration;
        }
    }
}
