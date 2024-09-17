using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Utils;
using VRBuilder.TextToSpeech;
using VRBuilder.Core.Localization;
using UnityEngine.Localization.Settings;
using VRBuilder.Editor.TextToSpeech;
using System.Threading.Tasks;
using Source.TextToSpeech_Component.Runtime;
using UnityEngine.Localization;

namespace VRBuilder.Editor.TextToSpeech.UI
{
    /// <summary>
    /// This class draws list of <see cref="ITextToSpeechProvider"/> in <see cref="textToSpeechSettings"/>.
    /// </summary>
    [CustomEditor(typeof(TextToSpeechSettings))]
    public class TextToSpeechSettingsEditor : UnityEditor.Editor
    {
        private TextToSpeechSettings textToSpeechSettings;
        
        private string[] providers = { "Empty" };
        private string streamingAssetCacheDirectoryName = "TextToSpeech";
        
        private ITextToSpeechProvider currentElement;
        private ITextToSpeechConfiguration currentElementSettings;
        private int providersIndex = 0;
        private int lastProviderSelectedIndex = 0;

        private void OnEnable()
        {
            textToSpeechSettings = (TextToSpeechSettings)target;
            streamingAssetCacheDirectoryName = textToSpeechSettings.StreamingAssetCacheDirectoryName;
            providers = ReflectionUtils.GetConcreteImplementationsOf<ITextToSpeechProvider>().ToList().Where(type => type != typeof(FileTextToSpeechProvider)).Select(type => type.Name).ToArray();
            lastProviderSelectedIndex = providersIndex = string.IsNullOrEmpty(textToSpeechSettings.Provider) ? Array.IndexOf(providers, nameof(MicrosoftSapiTextToSpeechProvider)) : Array.IndexOf(providers, textToSpeechSettings.Provider);
            textToSpeechSettings.Provider = providers[providersIndex];
            
            GetProviderInstance();
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            
            providersIndex = EditorGUILayout.Popup("Provider", providersIndex, providers);
            
            DrawDefaultInspector();
            
            //UnityEditor.Editor.CreateEditor(AWSTextToSpeechConfiguration.LoadConfiguration()).OnInspectorGUI();
            //
            
            if (providersIndex != lastProviderSelectedIndex)
            {
                lastProviderSelectedIndex = providersIndex;
                textToSpeechSettings.Provider = providers[providersIndex];
                
                GetProviderInstance();
                textToSpeechSettings.Save();
            }
            
            //UnityEditor.Editor.CreateEditor(currentElement ?? TextToSpeechDefaultConfiguration.Instance).OnInspectorGUI(); 
            
            if (currentElementSettings is ScriptableObject scriptibleConfig)
            {
                //TODO research is this the right way ?
                UnityEditor.Editor.CreateEditor(scriptibleConfig).OnInspectorGUI();
            }
            
            IProcess currentProcess = GlobalEditorHandler.GetCurrentProcess();

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            if (GUILayout.Button(new GUIContent("Generate all TTS files only for Active Locale", $"Active Locale: {LanguageSettings.Instance.ActiveOrDefaultLocale?.Identifier}")))
            {
                TextToSpeechEditorUtils.GenerateTextToSpeechForAllProcessesAndActiveOrDefaultLocale();
            }

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
           
            string tooltip = LocalizationSettings.HasSettings ? GetAvailableLocales() : string.Empty;
            if (GUILayout.Button(new GUIContent("Generate all TTS files for all Available Locales", $"Available Locales: {tooltip}")))
            {
                TextToSpeechEditorUtils.GenerateTextToSpeechForAllProcesses();
            }

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            if (GUILayout.Button("Delete generated TTS files"))
            {
                if (EditorUtility.DisplayDialog("Delete TTS files", "All generated text-to-speech files will be deleted. Proceed?", "Yes", "No"))
                {
                    string absolutePath = Application.streamingAssetsPath;
                    string relativePath = absolutePath.Replace(Application.dataPath, "Assets");
                    string directory = Path.Combine(relativePath, streamingAssetCacheDirectoryName);

                    if (AssetDatabase.DeleteAsset(directory))
                    {
                        Debug.Log("TTS cache flushed.");
                    }
                    else
                    {
                        Debug.Log("No TTS cache to flush.");
                    }
                }
            }
        }

        private string GetAvailableLocales()
        {
            string availableLOcalesString = "";
            for (int i = 0; LocalizationSettings.AvailableLocales != null && i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
            {
                availableLOcalesString += "\n" + LocalizationSettings.AvailableLocales.Locales[i].Identifier;
            }
            return availableLOcalesString;
        }
        
        private void GetProviderInstance()
        {
            var currentProviderType = ReflectionUtils.GetConcreteImplementationsOf<ITextToSpeechProvider>().FirstOrDefault(type => type.Name == providers[providersIndex]);
            if (Activator.CreateInstance(currentProviderType) is ITextToSpeechProvider provider)
            {
                currentElement = provider;
                currentElementSettings = currentElement.LoadConfig();
            }
        }
    }
}