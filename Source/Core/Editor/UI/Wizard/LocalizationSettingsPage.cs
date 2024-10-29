using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.UI;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace VRBuilder.Core.Editor.UI.Wizard
{
    /// <summary>
    /// Wizard page where the user can set up localization preferences.
    /// </summary>
    internal class LocalizationSettingsPage : WizardPage
    {
        private static Texts s_Texts;

        private bool projectSettingsOpened = false;

        [SerializeField]
        private bool skipLocalization = true;

        public LocalizationSettingsPage() : base("Localization Settings")
        {
            projectSettingsOpened = false;
        }

        public override void Apply()
        {
            base.Apply();
        }

        public override void Draw(Rect window)
        {
            GUILayout.BeginArea(window);

            GUILayout.Label("Unity Localization", BuilderEditorStyles.Title);

            if (GUILayout.Toggle(skipLocalization, "Use a single language in this project", BuilderEditorStyles.RadioButton))
            {
                skipLocalization = true;
            }

            if (GUILayout.Toggle(!skipLocalization, "Configure Unity localization to create a multi-language application", BuilderEditorStyles.RadioButton))
            {
                skipLocalization = false;
            }

            if (skipLocalization == false)
            {
                DisplayLocalizationInstructions();
            }
            GUILayout.EndArea();
        }

        private void DisplayLocalizationInstructions()
        {
            GUILayout.Label("For VR Builder Localization it is recommended to do the following steps:", BuilderEditorStyles.Paragraph);

            DrawHowToCreateLocalizationSettingsFile();
            DrawHowToCreateLanguagesAndLocales();
            DrawHowToChooseDefaultLanguageAndLocale();
            DrawHowToCreateStringTableCollection();
            DrawSelectLocalizationTable();

            GUILayout.Space(16);

            if (LocalizationEditorSettings.ActiveLocalizationSettings == null)
            {
                DrawCreateLocalizationSettingsFile();
            }
            else
            {
                DrawProjectSettingsLocalization();
            }
        }

        private void DrawHowToCreateLocalizationSettingsFile()
        {
            GUILayout.BeginHorizontal();
            if (LocalizationEditorSettings.ActiveLocalizationSettings != null)
            {
                ShowCheckMarkToggle();
            }
            else
            {
                GUILayout.Space(16);
            }
            GUILayout.Label("1. Create a Localization Settings File", BuilderEditorStyles.Paragraph);
            GUILayout.EndHorizontal();
            BuilderGUILayout.DrawLink("Quick Start Guide: Create the Localization Settings", "https://docs.unity3d.com/Packages/com.unity.localization@1.0/manual/QuickStartGuideWithVariants.html#create-the-localization-settings", BuilderEditorStyles.IndentLarge);
        }

        private void DrawHowToCreateLanguagesAndLocales()
        {
            GUILayout.BeginHorizontal();
            if ((LocalizationEditorSettings.ActiveLocalizationSettings != null && LocalizationEditorSettings.ActiveLocalizationSettings.GetAvailableLocales() != null && LocalizationEditorSettings.ActiveLocalizationSettings.GetAvailableLocales().Locales.Count > 0)
               || (LocalizationSettings.HasSettings && (LocalizationSettings.AvailableLocales.Locales.Count > 0 || LocalizationSettings.SelectedLocale != null || LocalizationSettings.ProjectLocale != null)))
            {
                ShowCheckMarkToggle();
            }
            else
            {
                GUILayout.Space(16);
            }
            GUILayout.Label("2. Create Languages / Locales", BuilderEditorStyles.Paragraph);
            GUILayout.EndHorizontal();
            BuilderGUILayout.DrawLink("Quick Start Guide: Create locales", "https://docs.unity3d.com/Packages/com.unity.localization@1.0/manual/QuickStartGuideWithVariants.html#create-locales", BuilderEditorStyles.IndentLarge);
        }

        private void DrawHowToChooseDefaultLanguageAndLocale()
        {
            GUILayout.BeginHorizontal();
            if (LocalizationEditorSettings.ActiveLocalizationSettings != null && LocalizationSettings.ProjectLocale != null)
            {
                ShowCheckMarkToggle();
            }
            else
            {
                GUILayout.Space(16);
            }
            GUILayout.Label("3. Choose a default Language / Locale", BuilderEditorStyles.Paragraph);
            GUILayout.EndHorizontal();
            BuilderGUILayout.DrawLink("Quick Start Guide: Choose a default Locale", "https://docs.unity3d.com/Packages/com.unity.localization@1.0/manual/QuickStartGuideWithVariants.html#choose-a-default-locale", BuilderEditorStyles.IndentLarge);
        }

        private void DrawHowToCreateStringTableCollection()
        {
            GUILayout.BeginHorizontal();
            if (LocalizationEditorSettings.GetStringTableCollections().Count > 0)
            {
                ShowCheckMarkToggle();
            }
            else
            {
                GUILayout.Space(16);
            }
            GUILayout.Label("4. Create a String Table Collection", BuilderEditorStyles.Paragraph);
            GUILayout.EndHorizontal();
            BuilderGUILayout.DrawLink("Quick Start Guide: Localize Strings", "https://docs.unity3d.com/Packages/com.unity.localization@1.0/manual/QuickStartGuideWithVariants.html#localize-strings", BuilderEditorStyles.IndentLarge);
        }

        private void DrawSelectLocalizationTable()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(16);
            GUILayout.Label("5. Select the string table just created on the PROCESS_CONFIGURATION\n    game object in your scene.", BuilderEditorStyles.Paragraph);
            GUILayout.EndHorizontal();
        }

        private void DrawCreateLocalizationSettingsFile()
        {
            GUILayout.Label("Create a Localization Settings File", BuilderEditorStyles.Header);
            GUILayout.Label("Click the button to create a Localization Settings File.", BuilderEditorStyles.Paragraph);

            GUILayout.Space(8);

            if (s_Texts == null)
                s_Texts = new Texts();

            EditorGUI.BeginChangeCheck();
            var obj = EditorGUILayout.ObjectField(s_Texts.activeSettings, LocalizationEditorSettings.ActiveLocalizationSettings, typeof(LocalizationSettings), false) as LocalizationSettings;
            if (EditorGUI.EndChangeCheck())
            {
                LocalizationEditorSettings.ActiveLocalizationSettings = obj;
            }
            EditorGUILayout.HelpBox(s_Texts.noSettingsMsg.text, MessageType.Info, true);
            if (GUILayout.Button("Create Localization Settings", GUILayout.Width(300)))
            {
                var created = CreateLocalizationAsset();
                if (created != null)
                {
                    LocalizationEditorSettings.ActiveLocalizationSettings = created;
                }
            }
        }

        private LocalizationSettings CreateLocalizationAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save Localization Settings", "Localization Settings", "asset", "Please enter a filename to save the projects localization settings to.");

            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var settings = ScriptableObject.CreateInstance<LocalizationSettings>();
            settings.name = "VR Builder Localization Settings";

            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();
            return settings;
        }

        private void DrawProjectSettingsLocalization()
        {
            GUILayout.Label("Project Settings: Localization", BuilderEditorStyles.Header);
            GUILayout.Label("Use the Locale Generator to add new Languages to the Available Locales List.", BuilderEditorStyles.Paragraph);

            if (GUILayout.Button("Open Localization Project Settings", GUILayout.Width(300)))
            {
                var settingsWindow = SettingsService.OpenProjectSettings("Project/Localization");
                projectSettingsOpened = true;
            }

            GUILayout.Space(8);

            if (projectSettingsOpened || (LocalizationSettings.AvailableLocales != null && LocalizationSettings.AvailableLocales.Locales.Count > 0))
            {
                GUILayout.Label("Localization Tables", BuilderEditorStyles.Header);
                GUILayout.Label("It is recommended to start with creating a new String Table Collection.", BuilderEditorStyles.Paragraph);

                if (GUILayout.Button("Open Localization Tables Window", GUILayout.Width(300)))
                {
                    LocalizationTablesWindow.ShowWindow();
                }
            }
        }

        private void ShowCheckMarkToggle()
        {
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            GUI.enabled = false;
            GUILayout.Toggle(true, "", GUILayout.Width(8));
            GUI.enabled = true;
            GUI.backgroundColor = backgroundColor;
        }

        private class Texts
        {
            public GUIContent activeSettings = EditorGUIUtility.TrTextContent("Active Settings", "The Localization Settings that will be used by this project and included into any builds.");
            public GUIContent noSettingsMsg = EditorGUIUtility.TrTextContent("You have no active Localization Settings. Would you like to create one?");
        }
    }
}
