using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;
using VRBuilder.Core.Editor.TextToSpeech.Providers;
using VRBuilder.Core.Editor.TextToSpeech.Utils;
using VRBuilder.Core.Settings;
using VRBuilder.Core.TextToSpeech;
using VRBuilder.Core.TextToSpeech.Configuration;
using VRBuilder.Core.TextToSpeech.Providers;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.UI.ProjectSettings
{
    /// <summary>
    /// This class draws list of <see cref="ITextToSpeechProvider"/> in <see cref="textToSpeechSettings"/> and other properties for text to speech settings.
    /// If an Implementation of a <see cref="ITextToSpeechProvider"/> is selected the linked <see cref="currentElementSettings"/> gets loaded.
    /// </summary>
    [CustomEditor(typeof(TextToSpeechSettings))]
    public class TextToSpeechSettingsEditor : UnityEditor.Editor
    {
        private TextToSpeechSettings textToSpeechSettings;

        private string[] providers = { "Empty" };
        private string cacheDirectoryName = "TextToSpeech";

        private ITextToSpeechProvider currentElement;
        private ITextToSpeechConfiguration currentElementSettings;

        private string lastSelectedCacheDirectory = "";
        private int providersIndex = 0;
        private int lastProviderSelectedIndex = 0;

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            providersIndex = EditorGUILayout.Popup("Provider", providersIndex, providers);
            lastSelectedCacheDirectory = EditorGUILayout.TextField("Streaming Asset Cache Directory Name", lastSelectedCacheDirectory);

            if (lastSelectedCacheDirectory != cacheDirectoryName)
            {
                cacheDirectoryName = lastSelectedCacheDirectory;
                textToSpeechSettings.StreamingAssetCacheDirectoryName = lastSelectedCacheDirectory;
            }

            //check if a new provider is selected
            if (providersIndex != lastProviderSelectedIndex)
            {
                lastProviderSelectedIndex = providersIndex;

                GetProviderInstance();
                //save new config
                textToSpeechSettings.Provider = providers[providersIndex];
                textToSpeechSettings.Configuration = currentElementSettings;

                textToSpeechSettings.Save();
            }

            //check selected element is 
            if (currentElementSettings is ScriptableObject scriptableObject)
            {
                GUILayout.Space(8);

                var customHeader = BuilderEditorStyles.Header;
                customHeader.fixedHeight = 25f;
                EditorGUILayout.LabelField("Provider specific settings", customHeader);
                GUILayout.Space(8);
                GUILayout.Label("Configuration for your Text to Speech provider.", BuilderEditorStyles.ApplyPadding(BuilderEditorStyles.Label, 0));
                UnityEditor.Editor.CreateEditor(scriptableObject).OnInspectorGUI();
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
                    string directory = Path.Combine(relativePath, cacheDirectoryName);

                    if (AssetDatabase.DeleteAsset(directory))
                    {
                        UnityEngine.Debug.Log("TTS cache flushed.");
                    }
                    else
                    {
                        UnityEngine.Debug.Log("No TTS cache to flush.");
                    }
                }
            }
        }

        private void OnEnable()
        {
            textToSpeechSettings = (TextToSpeechSettings)target;
            cacheDirectoryName = textToSpeechSettings.StreamingAssetCacheDirectoryName;
            lastSelectedCacheDirectory = cacheDirectoryName;
            providers = ReflectionUtils.GetConcreteImplementationsOf<ITextToSpeechProvider>().ToList().Where(type => type != typeof(FileTextToSpeechProvider)).Select(type => type.Name).ToArray();
            lastProviderSelectedIndex = providersIndex = string.IsNullOrEmpty(textToSpeechSettings.Provider) ? Array.IndexOf(providers, nameof(MicrosoftSapiTextToSpeechProvider)) : Array.IndexOf(providers, textToSpeechSettings.Provider);
            textToSpeechSettings.Provider = providers[providersIndex];

            GetProviderInstance();
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

        private string GetAvailableLocales()
        {
            string availableLOcalesString = "";
            for (int i = 0; LocalizationSettings.AvailableLocales != null && i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
            {
                availableLOcalesString += "\n" + LocalizationSettings.AvailableLocales.Locales[i].Identifier;
            }
            return availableLOcalesString;
        }
    }
}
