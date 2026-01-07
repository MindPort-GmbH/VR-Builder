using System;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.Settings;
using VRBuilder.Core.TextToSpeech.Providers;
using VRBuilder.Core.Utils;

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

        private Type currentProviderType;
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
            // clear elements if a new provider is selected
            if (currentProvider?.GetType().Name != Provider)
            {
                currentProviderType = null;
                currentProvider = null;
            }
            
            currentProviderType ??= ReflectionUtils.GetConcreteImplementationsOf<ITextToSpeechProvider>().FirstOrDefault(type => type.Name == Provider);
            
            if (currentProviderType != null)
            {
                currentProvider ??= (ITextToSpeechProvider)Activator.CreateInstance(currentProviderType);
            }

            return currentProvider;
        }
    }
}