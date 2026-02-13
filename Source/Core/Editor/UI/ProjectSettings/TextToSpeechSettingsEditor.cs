using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Source.Core.Runtime.TextToSpeech;
using Source.Core.Runtime.TextToSpeech.Utils.VRBuilder.Core.TextToSpeech;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;
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
        private string[] providersSpeaker = { "Empty" };
        private string cacheDirectoryName = "TextToSpeech";

        private IProcess currentActiveProcess;
        private Type currentProviderType;
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
            // Draw only profile if they are supported by at least one text-to-speech provider that implements ITextToSpeechSpeaker
            if (ReflectionUtils.GetConcreteImplementationsOf<ITextToSpeechSpeaker>().Any())
            {
                DrawVoiceProfilesSection();
            }

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
            providers = ReflectionUtils.GetConcreteImplementationsOf<ITextToSpeechProvider>().Where(type => type != typeof(FileTextToSpeechProvider)).Select(type => type.Name).ToArray();
            providersSpeaker = ReflectionUtils.GetConcreteImplementationsOf<ITextToSpeechSpeaker>().Select(type => type.Name).ToArray();
            lastProviderSelectedIndex = providersIndex = string.IsNullOrEmpty(textToSpeechSettings.Provider) ? Array.IndexOf(providers, nameof(MicrosoftSapiTextToSpeechProvider)) : Array.IndexOf(providers, textToSpeechSettings.Provider);
            
            // Check if the latest index is greater than the count of providers
            if (providersIndex >= providers.Length || providersIndex < 0)
            {
                lastProviderSelectedIndex = providersIndex = 0;
            }
            
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
        
        private void SavePrefs()
        {
            EditorPrefs.SetInt(PrefKeyScope, (int)scope);
            EditorPrefs.SetInt(PrefKeyLanguage, (int)language);
        }
        
        private void DrawVoiceProfilesSection()
        {
            EditorGUILayout.LabelField("Voice Profiles", CustomHeader);
            GUILayout.Space(8);

            EditorGUILayout.HelpBox("Voice profiles map languages to specific voices for each Text-To-Speech provider. Create profiles to define which voice should be used for each language.\nIf the Text-To-Speech provider supports multiple voices", MessageType.Info);

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
            
            if (textToSpeechSettings.VoiceProfiles.Length <= 0)
            {
                EditorGUILayout.HelpBox("No voice profiles configured. Add a profile to get started.", MessageType.Warning);
            }
            else
            {
                DrawProfileTable();
            }
            
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        private Dictionary<string, ITextToSpeechSpeaker> speakerProvidersCache = new();

        private void DrawVoiceIdDropdown(ProviderVoiceMapping mapping)
        {
            if (!speakerProvidersCache.TryGetValue(mapping.ProviderName, out var speakerProvider))
            {
                var providerType = ReflectionUtils.GetConcreteImplementationsOf<ITextToSpeechSpeaker>().FirstOrDefault(t => t.Name == mapping.ProviderName);
                if (providerType != null)
                {
                    var instance = Activator.CreateInstance(providerType) as ITextToSpeechProvider ?? new MicrosoftSapiTextToSpeechProvider();
                    instance.LoadConfig();
                    speakerProvider = instance as ITextToSpeechSpeaker;
                }
                speakerProvidersCache[mapping.ProviderName] = speakerProvider;
            }

            if (speakerProvider != null)
            {
                List<string> speakers = speakerProvider.GetSpeaker();
                int speakerIndex = speakers.IndexOf(mapping.VoiceId);
                int newSpeakerIndex = EditorGUILayout.Popup(speakerIndex, speakers.ToArray(), GUILayout.Width(120));
                if (newSpeakerIndex != speakerIndex && newSpeakerIndex >= 0)
                {
                    mapping.VoiceId = speakers[newSpeakerIndex];
                    EditorUtility.SetDirty(textToSpeechSettings);
                }
                else if(speakerIndex == -1 && speakers.Count > 0)
                {
                    mapping.VoiceId = speakers[0];
                    EditorUtility.SetDirty(textToSpeechSettings);
                }
            }
        }
        
        private void DrawProfileTable()
        {
            // Table header
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Display Name", EditorStyles.boldLabel, GUILayout.Width(120));
            GUILayout.Label("Languages", EditorStyles.boldLabel, GUILayout.Width(100));
            GUILayout.Label("Provider & Voice ID Mappings", EditorStyles.boldLabel);
            GUILayout.Label("Actions", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.EndHorizontal();

            // Profile rows in a scroll view
            EditorGUILayout.BeginVertical();

            for (var i = 0; i < textToSpeechSettings.VoiceProfiles.Length; i++)
            {
                var profile = textToSpeechSettings.VoiceProfiles[i];
                // Begin horizontal group
                EditorGUILayout.BeginHorizontal(GUI.skin.box);

                // Display Name
                EditorGUI.BeginChangeCheck();
                string newDisplayName = EditorGUILayout.TextField(profile.DisplayName, GUILayout.Width(120));
                if (EditorGUI.EndChangeCheck())
                {
                    profile.DisplayName = newDisplayName;
                    EditorUtility.SetDirty(textToSpeechSettings);
                    textToSpeechSettings.TriggerVoiceProfilesChanged();
                }

                // Language Codes
                if (LocalizationSettings.HasSettings)
                {
                    var locales = LocalizationSettings.AvailableLocales.Locales;
                    var localeCodes = locales.Select(l => l.Identifier.Code).ToList();
                    localeCodes.Insert(0, "all");
                    
                    // Draw selection
                    string languagesString = profile.LanguageCode is { Length: > 0 } ? string.Join(", ", profile.LanguageCode) : "all";
                    if (GUILayout.Button(languagesString, EditorStyles.layerMaskField, GUILayout.Width(100)))
                    {
                        GenericMenu menu = new GenericMenu();
                        foreach (var code in localeCodes)
                        {
                            menu.AddItem(new GUIContent(code), profile.LanguageCode != null && profile.LanguageCode.Contains(code), () =>
                            {
                                AddLanguageToVoiceProfile(profile, code);
                            });
                        }
                        menu.ShowAsContext();
                    }
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.TextField("No Languages", GUILayout.Width(100));
                    EditorGUI.EndDisabledGroup();
                }

                // Mappings
                EditorGUILayout.BeginVertical();
                for (int j = 0; j < profile.ProviderVoiceMappings.Count; j++)
                {
                    var mapping = profile.ProviderVoiceMappings[j];
                    EditorGUILayout.BeginHorizontal();
                    
                    // Provider
                    int providerIndex = Array.IndexOf(providersSpeaker, mapping.ProviderName);
                    int newProviderIndex = EditorGUILayout.Popup(providerIndex, providersSpeaker, GUILayout.Width(150));
                    if (newProviderIndex != providerIndex && newProviderIndex >= 0)
                    {
                        mapping.ProviderName = providersSpeaker[newProviderIndex];
                        EditorUtility.SetDirty(textToSpeechSettings);
                    }

                    // Voice ID
                    DrawVoiceIdDropdown(mapping);

                    // Remove Mapping
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        profile.ProviderVoiceMappings.RemoveAt(j);
                        EditorUtility.SetDirty(textToSpeechSettings);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Mapping", GUILayout.Width(100)))
                {
                    profile.ProviderVoiceMappings.Add(new ProviderVoiceMapping(providers.Length > 0 ? currentElement.GetType().Name : "", ""));
                    EditorUtility.SetDirty(textToSpeechSettings);
                }
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    RemoveVoiceProfile(i);
                    // Break since we modified the collection
                    EditorGUILayout.EndHorizontal();
					break;
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(4);
            }

            EditorGUILayout.EndVertical();
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
            int newScopeIndex = GUILayout.Toolbar((int)scope, new[] { new GUIContent("Active Scene"), new GUIContent("All Scenes with Processes") }, customToggle);
            ScopeOption newScope = (ScopeOption)newScopeIndex;

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            GUILayout.Label("Locales");
            int newLangIndex = GUILayout.Toolbar((int)language, new[] { new GUIContent("Current Language"), new GUIContent("All Languages") }, customToggle);
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
        
        private void AddVoiceProfile()
        {
            var profiles = textToSpeechSettings.VoiceProfiles.ToList();
            profiles.Add(new VoiceProfile("Default Profile", new[] { "all" }, "", new []{currentElement.GetType().Name}));
            textToSpeechSettings.VoiceProfiles = profiles.ToArray();
            EditorUtility.SetDirty(textToSpeechSettings);
            textToSpeechSettings.Save();
        }
        
        private void AddLanguageToVoiceProfile(VoiceProfile profile, string newCodeToAdd)
        {
            var list = profile.LanguageCode != null ? profile.LanguageCode.ToList() : new List<string>();
            
            // Checks if the element is new in the list
            if (list.Contains(newCodeToAdd))
            {
                // Was in the list before
                list.Remove(newCodeToAdd);
            }
            else
            {
                // Was not in the List before
                if (newCodeToAdd == "all")
                {
                    list.Clear();
                }
                else
                {
                    // Removes the all tag if an other language was selected
                    list.Remove("all");
                }
                list.Add(newCodeToAdd);
            }
            
            profile.LanguageCode = list.ToArray();
            EditorUtility.SetDirty(textToSpeechSettings);
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
        
        private void GetProviderInstance()
        {
            currentProviderType ??= ReflectionUtils.GetConcreteImplementationsOf<ITextToSpeechProvider>().FirstOrDefault(type => type.Name == TextToSpeechSettings.Instance.Provider);
            if (currentElement == null && currentProviderType != null && Activator.CreateInstance(currentProviderType) is ITextToSpeechProvider provider)
            {
                currentElement = provider;
                currentElementSettings = currentElement.LoadConfig();
            }
        }
    }
}
