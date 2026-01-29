using System;
using System.IO;
using System.Linq;
using Source.Core.Runtime.TextToSpeech.Utils.VRBuilder.Core.TextToSpeech;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Editor.TextToSpeech.Providers;
using VRBuilder.Core.Editor.TextToSpeech.Utils;
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
        private bool generateAudioInBuildingProcess;

        // Text to speech provider management
        private string lastSelectedCacheDirectory = "";
        private int providersIndex = 0;
        private int lastProviderSelectedIndex = 0;
        
        // Voice profile management
        private Vector2 profileScrollPosition;
        
        // Build and file management
        private enum ScopeOption { ActiveScene = 0, AllProcesses = 1 }
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

            if (!generateAudioInBuildingProcess)
            {
                EditorGUILayout.HelpBox("Text-to-speech files will not be generated during the building process. Text-to-speech files must be generated manually.", MessageType.Warning);
            }
            
            if (lastSelectedCacheDirectory != cacheDirectoryName)
            {
                cacheDirectoryName = lastSelectedCacheDirectory;
                textToSpeechSettings.StreamingAssetCacheDirectoryName = lastSelectedCacheDirectory;
            }

            if (generateAudioInBuildingProcess != textToSpeechSettings.GenerateAudioInBuildingProcess)
            {
                textToSpeechSettings.GenerateAudioInBuildingProcess = generateAudioInBuildingProcess;
            }
            
            // Voice Profiles Section
            DrawVoiceProfilesSection();
            
            // Text to speech provider settings
            DrawTextToSpeechProviderSelection();
            
            // Text to speech actions
            DrawTextToSpeechActionsSection();

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
            currentElement = TextToSpeechProviderFactory.Instance.CreateProvider();
            currentElementSettings = currentElement.LoadConfig();
        }
        
        private void SavePrefs()
        {
            EditorPrefs.SetInt(PrefKeyScope, (int)scope);
            EditorPrefs.SetInt(PrefKeyLanguage, (int)language);
        }
        
        private void DrawVoiceProfilesSection()
        {
            EditorGUILayout.LabelField("Voice Profiles", CustomHeader);
            GUILayout.Space(8);
            
            EditorGUILayout.HelpBox("Voice profiles map languages to specific voices for each TTS provider. Create profiles to define which voice should be used for each language.", MessageType.Info);
            
            GUILayout.Space(4);
            
            // Add/Remove buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Profile", GUILayout.Width(100)))
            {
                AddVoiceProfile();
            }
            
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(4);
            
            // Profile table
            if (textToSpeechSettings.VoiceProfiles.Length == 0)
            {
                EditorGUILayout.HelpBox("No voice profiles configured. Add a profile to get started.", MessageType.Warning);
            }
            else
            {
                DrawProfileTable();
            }
            
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }
        
        private void DrawProfileTable()
        {
            // Table header
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Display Name", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Languages", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Voice ID", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Provider", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Actions", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            // Profile rows in a scroll view
            EditorGUILayout.BeginVertical();

            for (var i = 0; i < textToSpeechSettings.VoiceProfiles.Length; i++)
            {
                var profile = textToSpeechSettings.VoiceProfiles[i];
                // Begin horizontal group
                EditorGUILayout.BeginHorizontal();

                // Draw selection highlight
                if (Event.current.type == EventType.Repaint)
                {
                    GUIStyle style = GUIStyle.none;
                    style.Draw(new Rect(), false, false, false, false);
                }

                // Display Name
                EditorGUI.BeginChangeCheck();
                string newDisplayName = EditorGUILayout.TextField(profile.DisplayName, GUILayout.Width(150));
                if (EditorGUI.EndChangeCheck())
                {
                    profile.DisplayName = newDisplayName;
                    EditorUtility.SetDirty(textToSpeechSettings);
                }

                // Language Codes
                EditorGUI.BeginChangeCheck();
                string languagesString = profile.LanguageCode != null ? string.Join(", ", profile.LanguageCode) : "";
                string newLanguagesString = EditorGUILayout.TextField(languagesString, GUILayout.Width(150));
                if (EditorGUI.EndChangeCheck())
                {
                    profile.LanguageCode = newLanguagesString
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToArray();
                    EditorUtility.SetDirty(textToSpeechSettings);
                }

                // Voice ID
                EditorGUI.BeginChangeCheck();
                string newVoiceId = EditorGUILayout.TextField(profile.VoiceId, GUILayout.Width(150));
                if (EditorGUI.EndChangeCheck())
                {
                    profile.VoiceId = newVoiceId;
                    EditorUtility.SetDirty(textToSpeechSettings);
                }

                // Providers
                EditorGUI.BeginChangeCheck();
                string providersString = profile.ProviderNames != null ? string.Join(", ", profile.ProviderNames) : "";
                string newProvidersString = EditorGUILayout.TextField(providersString, GUILayout.Width(150));
                if (EditorGUI.EndChangeCheck())
                {
                    profile.ProviderNames = newProvidersString
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToArray();
                    EditorUtility.SetDirty(textToSpeechSettings);
                }

                if (GUILayout.Button("Remove Profile", GUILayout.Width(120)))
                {
                    RemoveVoiceProfile(i);
                }

                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(4);
            }

            EditorGUILayout.EndVertical();
        }

        private void AddVoiceProfile()
        {
            var profiles = textToSpeechSettings.VoiceProfiles.ToList();
            profiles.Add(new VoiceProfile("New Profile", new[] { "en-US" }, "", providers));
            textToSpeechSettings.VoiceProfiles = profiles.ToArray();
            EditorUtility.SetDirty(textToSpeechSettings);
            textToSpeechSettings.Save();
        }

        private void RemoveVoiceProfile(int index)
        {
            if (index < 0 || index >= textToSpeechSettings.VoiceProfiles.Length)
            {
                return;
            }
                
            var profiles = textToSpeechSettings.VoiceProfiles.ToList();
            profiles.RemoveAt(index);
            textToSpeechSettings.VoiceProfiles = profiles.ToArray();
            EditorUtility.SetDirty(textToSpeechSettings);
            textToSpeechSettings.Save();
        }
        
        private void DrawTextToSpeechProviderSelection()
        {
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
        }
        
        private void DrawTextToSpeechActionsSection()
        {

            EditorGUILayout.LabelField("Text To Speech generation actions", CustomHeader);

            // Scope toolbar
            GUILayout.Label("What scope to generate for");
            EditorGUI.BeginChangeCheck();
            int newScopeIndex = 
                GUILayout.Toolbar(
                    (int)scope, new[] { new GUIContent("Active Scene"), new GUIContent("All Scenes with Processes") }
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
                    case ScopeOption.AllProcesses:
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
        }
    }
}
