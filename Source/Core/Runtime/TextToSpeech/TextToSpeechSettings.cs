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
        /// Configuration of the <see cref="ITextToSpeechProvider"/>.
        /// </summary>
        [HideInInspector]
        public ITextToSpeechConfiguration Configuration;

        /// <summary>
        /// StreamingAsset directory name which is used to load/save audio files.
        /// </summary>
        public string StreamingAssetCacheDirectoryName = "TextToSpeech";

        public TextToSpeechSettings()
        {
            Provider = "MicrosoftSapiTextToSpeechProvider";
        }

        /// <summary>
        /// Loads the first existing <see cref="MicrosoftTextToSpeechConfiguration"/> found in the project.
        /// If any <see cref="MicrosoftTextToSpeechConfiguration"/> exist in the project it creates and saves a new instance with default values (editor only).
        /// </summary>
        /// <remarks>When used in runtime, this method can only retrieve config files located under a Resources folder.</remarks>
        //public static MicrosoftTextToSpeechConfiguration LoadConfiguration()
        //{
        //    MicrosoftTextToSpeechConfiguration configuration = Resources.Load<MicrosoftTextToSpeechConfiguration>(nameof(MicrosoftTextToSpeechConfiguration));
        //    return configuration != null ? configuration : CreateNewConfiguration();
        //}
        private ITextToSpeechConfiguration CreateNewConfiguration()
        {
            ITextToSpeechConfiguration config = CreateInstance(Provider) as ITextToSpeechConfiguration;
            RuntimeConfigurator.Configuration.SetTextToSpeechConfiguration(config);

#if UNITY_EDITOR
            string resourcesPath = "Assets/MindPort/VR Builder/Resources/";
            string configFilePath = $"{resourcesPath}{config.GetType().Name}.asset";

            if (Directory.Exists(resourcesPath) == false)
            {
                Directory.CreateDirectory(resourcesPath);
            }

            Debug.LogWarningFormat("No text to speech configuration found!\nA new configuration file was created at {0}", configFilePath);
            UnityEditor.AssetDatabase.CreateAsset((ScriptableObject)config, configFilePath);
            UnityEditor.AssetDatabase.Refresh();

            if (Application.isPlaying == false)
            {
                UnityEditor.Selection.activeObject = (ScriptableObject)config;
            }
#endif

            return config;
        }
    }
}
