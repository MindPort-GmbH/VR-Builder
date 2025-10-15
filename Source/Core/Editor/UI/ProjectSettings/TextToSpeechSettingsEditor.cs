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

        private IProcess currentActiveProcess;
        private ITextToSpeechProvider currentElement;
        private ITextToSpeechConfiguration currentElementSettings;

        private string lastSelectedCacheDirectory = "";
        private int providersIndex = 0;
        private int lastProviderSelectedIndex = 0;
        private bool generateAudioInBuildingProcess;
        
        private enum ScopeOption { ActiveScene = 0, BuildScenes = 1, AllProcesses = 2 }
        private enum LanguageOption { Current = 0, All = 1 }
        
        private ScopeOption scope = ScopeOption.ActiveScene;
        private LanguageOption language = LanguageOption.Current;
        
        private readonly GUILayoutOption buttonStyling = GUILayout.Width(200);
        private readonly GUILayoutOption customToggle = GUILayout.Width(405);
        private static GUIStyle customHeader;
        
        private const string PrefKeyScope = "VRB_TTS_Scope";
        private const string PrefKeyLanguage = "VRB_TTS_Language";

        public static GUIStyle CustomHeader
        {
            get
            {
                if (customHeader == null)
                {
                    customHeader = new GUIStyle(BuilderEditorStyles.Header);
                    customHeader.fixedHeight = 25f;
                }

                return customHeader;
            }
        }
        
        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            lastSelectedCacheDirectory = EditorGUILayout.TextField(new GUIContent("Cache Directory Name", "Name for the streaming asset cache directory for TTS files"), lastSelectedCacheDirectory);
            generateAudioInBuildingProcess = EditorGUILayout.Toggle(new GUIContent("Generate TTS while Building", "If checked, text-to-speech audio will be generated during the building process, otherwise it won't generate TTS audio during the building process."), generateAudioInBuildingProcess);

            if (lastSelectedCacheDirectory != cacheDirectoryName)
            {
                cacheDirectoryName = lastSelectedCacheDirectory;
                textToSpeechSettings.StreamingAssetCacheDirectoryName = lastSelectedCacheDirectory;
            }

            if (generateAudioInBuildingProcess != textToSpeechSettings.GenerateAudioInBuildingProcess)
            {
                textToSpeechSettings.GenerateAudioInBuildingProcess = generateAudioInBuildingProcess;
            }
            
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            
            EditorGUILayout.LabelField("Text To Speech Provider settings", CustomHeader);
            GUILayout.Space(8);
            
            providersIndex = EditorGUILayout.Popup("Provider", providersIndex, providers);
            
            //check if a new provider is selected
            if (providersIndex != lastProviderSelectedIndex)
            {
                lastProviderSelectedIndex = providersIndex;

                GetProviderInstance();
                //save new config in editor
                textToSpeechSettings.Provider = providers[providersIndex];

                textToSpeechSettings.Save();
            }
            
            //check selected element is 
            if (currentElementSettings is ScriptableObject scriptableObject)
            {
                GUILayout.Label("Configuration of your selcted Text to Speech provider.", BuilderEditorStyles.ApplyPadding(BuilderEditorStyles.Label, 0));
                CreateEditor(scriptableObject).OnInspectorGUI();
            }
            
            EditorGUILayout.LabelField("Text To Speech generation actions", CustomHeader);

            // Scope toolbar
            GUILayout.Label("What scope to generate for");
            EditorGUI.BeginChangeCheck();
            int newScopeIndex = 
                GUILayout.Toolbar(
                (int)scope, new[] { new GUIContent("Active Scene"), /*new GUIContent("Scenes in Build List"),*/ new GUIContent("All Scenes with Processes") }
            , customToggle);
            ScopeOption newScope = (ScopeOption)newScopeIndex;

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            GUILayout.Label("Locales");
            int newLangIndex = GUILayout.Toolbar(
                (int)language,
                new[] { new GUIContent("Current Language"), new GUIContent("All Languages") }
            , customToggle);
            language = (LanguageOption)newLangIndex;

            if (EditorGUI.EndChangeCheck())
            {
                scope = newScope;
                SavePrefs();
                Repaint();
            }
            
            GUILayout.Space(10f);
            
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("Generate TTS files"), buttonStyling))
            {
                SavePrefs();
                switch (scope)
                {
                    case ScopeOption.ActiveScene:
                        if (language == LanguageOption.Current)
                        {
                            _ = TextToSpeechEditorUtils.GenerateTextToSpeechActiveSceneAndActiveOrDefaultLocale();
                        }
                        else
                        {
                            _ = TextToSpeechEditorUtils.GenerateTextToSpeechActiveScene();
                        }
                        break;
                    //TODO add workable build scenes access function here as well
                    //case ScopeOption.BuildScenes:
                    //    if (language == LanguageOption.Current)
                    //    {
                    //        _ = TextToSpeechEditorUtils.GenerateTextToSpeechForBuildScenesAndActiveOrDefaultLocale();
                    //    }
                    //    else
                    //    {
                    //        _ = TextToSpeechEditorUtils.GenerateTextToSpeechForBuildScenes();
                    //    }
                    //    break;
                    case ScopeOption.BuildScenes or ScopeOption.AllProcesses:
                        if (language == LanguageOption.Current)
                        {
                            _ = TextToSpeechEditorUtils.GenerateTextToSpeechForAllProcessesAndActiveOrDefaultLocale();
                        }
                        else
                        {
                            _ = TextToSpeechEditorUtils.GenerateTextToSpeechForAllProcesses();
                        }

                        break;
                }
            }
            
            if (GUILayout.Button("Delete all generated TTS files", buttonStyling))
            {
                if (EditorUtility.DisplayDialog("Delete TTS files", "All generated text-to-speech files will be deleted. Proceed?", "Yes", "No"))
                {
                    string absolutePath = Application.streamingAssetsPath;
                    string relativePath = absolutePath.Replace(Application.dataPath, "Assets");
                    string directory = Path.Combine(relativePath, cacheDirectoryName);

                    UnityEngine.Debug.Log(AssetDatabase.DeleteAsset(directory)
                        ? "TTS cache flushed."
                        : "No TTS cache to flush.");
                }
            }
			
            GUILayout.EndHorizontal();
			
            GUILayout.Space(8);
        }

        private void OnEnable()
        {
            textToSpeechSettings = (TextToSpeechSettings)target;
            cacheDirectoryName = textToSpeechSettings.StreamingAssetCacheDirectoryName;
            lastSelectedCacheDirectory = cacheDirectoryName;
            providers = ReflectionUtils.GetConcreteImplementationsOf<ITextToSpeechProvider>().ToList().Where(type => type != typeof(FileTextToSpeechProvider)).Select(type => type.Name).ToArray();
            lastProviderSelectedIndex = providersIndex = string.IsNullOrEmpty(textToSpeechSettings.Provider) ? Array.IndexOf(providers, nameof(MicrosoftSapiTextToSpeechProvider)) : Array.IndexOf(providers, textToSpeechSettings.Provider);
            textToSpeechSettings.Provider = providers[providersIndex];
            generateAudioInBuildingProcess = textToSpeechSettings.GenerateAudioInBuildingProcess;


            if (EditorPrefs.HasKey(PrefKeyScope))
            {
                scope = (ScopeOption)EditorPrefs.GetInt(PrefKeyScope, (int)ScopeOption.ActiveScene);
            }

            if (EditorPrefs.HasKey(PrefKeyLanguage))
            {
                language = (LanguageOption)EditorPrefs.GetInt(PrefKeyLanguage, (int)LanguageOption.Current);
            }
            
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
        
        private void SavePrefs()
        {
            EditorPrefs.SetInt(PrefKeyScope, (int)scope);
            EditorPrefs.SetInt(PrefKeyLanguage, (int)language);
        }
    }
}
