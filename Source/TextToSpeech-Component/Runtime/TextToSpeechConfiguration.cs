using System;
using System.IO;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Runtime.Utils;
using UnityEngine;

namespace VRBuilder.TextToSpeech
{
    public class TextToSpeechConfiguration : SettingsObject<TextToSpeechConfiguration>
    {
        /// <summary>
        /// Name of the <see cref="ITextToSpeechProvider"/>.
        /// </summary>
        [HideInInspector]
        public string Provider;

        /// <summary>
        /// Language which should be used.
        /// </summary>
        /// <remarks>It depends on the chosen provider.</remarks>
        [HideInInspector]
        [Obsolete("Please use active language from the LanguageSettings.")]
        public string Language = "En";

        /// <summary>
        /// Voice that should be used.
        /// </summary>
        /// <remarks>It depends on the chosen provider.</remarks>
        public string Voice = "Male";

        /// <summary>
        /// Enables the usage of the streaming asset folder as second cache directory to allow deliveries which work offline too.
        /// This means the StreamingAssets/{StreamingAssetCacheDirectoryName} will be searched first and only if there
        /// is no fitting audio file found the text to speech provider will be used to download the audio file from web.
        /// </summary>
        public bool UseStreamingAssetFolder = true;

        /// <summary>
        /// With this option enabled the application tries to save the all downloaded audio files to
        /// StreamingAssets/{StreamingAssetCacheDirectoryName}.
        /// </summary>
        public bool SaveAudioFilesToStreamingAssets = false;

        /// <summary>
        /// StreamingAsset directory name which is used to load/save audio files.
        /// </summary>
        public string StreamingAssetCacheDirectoryName = "TextToSpeech";

        /// <summary>
        /// Used to authenticate at the provider, if required.
        /// </summary>
        public string Auth;

        public TextToSpeechConfiguration()
        {
            Provider = typeof(MicrosoftSapiTextToSpeechProvider).Name;
        }

        /// <summary>
        /// Loads the first existing <see cref="TextToSpeechConfiguration"/> found in the project.
        /// If any <see cref="TextToSpeechConfiguration"/> exist in the project it creates and saves a new instance with default values (editor only).
        /// </summary>
        /// <remarks>When used in runtime, this method can only retrieve config files located under a Resources folder.</remarks>
        public static TextToSpeechConfiguration LoadConfiguration()
        {
            TextToSpeechConfiguration configuration = Resources.Load<TextToSpeechConfiguration>(nameof(TextToSpeechConfiguration));
            return configuration != null ? configuration : CreateNewConfiguration();
        }
        
        private static TextToSpeechConfiguration CreateNewConfiguration()
        {
            TextToSpeechConfiguration textToSpeechConfiguration = CreateInstance<TextToSpeechConfiguration>();
            RuntimeConfigurator.Configuration.SetTextToSpeechConfiguration(textToSpeechConfiguration);
            
#if UNITY_EDITOR
            string resourcesPath = "Assets/MindPort/VR Builder/Resources/";
            string configFilePath = $"{resourcesPath}{nameof(TextToSpeechConfiguration)}.asset";
            
            if (Directory.Exists(resourcesPath) == false)
            {
                Directory.CreateDirectory(resourcesPath);
            }
            
            Debug.LogWarningFormat("No text to speech configuration found!\nA new configuration file was created at {0}", configFilePath);
            UnityEditor.AssetDatabase.CreateAsset(textToSpeechConfiguration, configFilePath);
            UnityEditor.AssetDatabase.Refresh();

            if (Application.isPlaying == false)
            {
                UnityEditor.Selection.activeObject = textToSpeechConfiguration;
            }
#endif
            
            return textToSpeechConfiguration;
        }
    }
}