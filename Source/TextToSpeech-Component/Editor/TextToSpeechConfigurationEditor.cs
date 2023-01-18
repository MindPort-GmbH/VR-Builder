using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Utils;
using VRBuilder.TextToSpeech;
using VRBuilder.TextToSpeech.Audio;

namespace VRBuilder.Editor.TextToSpeech.UI
{
    /// <summary>
    /// This class draws list of <see cref="ITextToSpeechProvider"/> in <see cref="textToSpeechConfiguration"/>.
    /// </summary>
    [CustomEditor(typeof(TextToSpeechConfiguration))]
    public class TextToSpeechConfigurationEditor : UnityEditor.Editor
    {
        private TextToSpeechConfiguration textToSpeechConfiguration;
        private string[] providers = { "Empty" };
        private int providersIndex = 0;
        private int lastProviderSelectedIndex = 0;

        private void OnEnable()
        {
            textToSpeechConfiguration = (TextToSpeechConfiguration)target;
            providers = ReflectionUtils.GetConcreteImplementationsOf<ITextToSpeechProvider>().ToList().Where(type => type != typeof(FileTextToSpeechProvider)).Select(type => type.Name).ToArray();
            lastProviderSelectedIndex = providersIndex = string.IsNullOrEmpty(textToSpeechConfiguration.Provider) ? Array.IndexOf(providers, nameof(MicrosoftSapiTextToSpeechProvider)) : Array.IndexOf(providers, textToSpeechConfiguration.Provider);
            textToSpeechConfiguration.Provider = providers[providersIndex];
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            providersIndex = EditorGUILayout.Popup("Provider", providersIndex, providers);
            
            if (providersIndex != lastProviderSelectedIndex)
            {
                lastProviderSelectedIndex = providersIndex;
                textToSpeechConfiguration.Provider = providers[providersIndex];
            }

            IProcess currentProcess = GlobalEditorHandler.GetCurrentProcess();

            if(GUILayout.Button("Generate all TTS files"))
            {
                IEnumerable<ITextToSpeechContent> ttsClips = EditorReflectionUtils.GetPropertiesFromProcess<ITextToSpeechContent>(currentProcess);

            }

            if (GUILayout.Button($"Generate audio files for process '{currentProcess.Data.Name}'"))
            {
                IEnumerable<ITextToSpeechContent> ttsClips = EditorReflectionUtils.GetPropertiesFromProcess<ITextToSpeechContent>(currentProcess).Where(clip => clip.IsCached == false);
                TextToSpeechEditorUtils.CacheTextToSpeechClips(ttsClips);
            }

            if(GUILayout.Button("Flush generated TTS files"))
            {
                if (EditorUtility.DisplayDialog("Flush TTS files", "All generated text-to-speech files will be deleted. Proceed?", "Yes", "No"))
                {
                    Directory.Delete(Path.Combine(Application.streamingAssetsPath, textToSpeechConfiguration.StreamingAssetCacheDirectoryName), true);
                    Debug.Log("TTS cache flushed.");
                }
            }
        }
    }
}