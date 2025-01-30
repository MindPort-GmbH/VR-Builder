using System;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.TextToSpeech;

namespace VRBuilder.Core.Editor.UI.ProjectSettings
{
    /// <summary>
    /// Provides text to speech settings.
    /// </summary>
    public class TextToSpeechSectionProvider : IProjectSettingsSection
    {
        /// <inheritdoc/>
        public string Title { get; } = "Text to Speech";

        /// <inheritdoc/>
        public Type TargetPageProvider { get; } = typeof(LanguageSettingsProvider);

        /// <inheritdoc/>
        public int Priority { get; } = 0;

        /// <inheritdoc/>
        public void OnGUI(string searchContext)
        {
            GUILayout.Label("General Configuration of Text to Speech.", BuilderEditorStyles.ApplyPadding(BuilderEditorStyles.Label, 0));

            GUILayout.Space(8);

            TextToSpeechSettings config = TextToSpeechSettings.Instance;
            UnityEditor.Editor.CreateEditor(config, typeof(TextToSpeechSettingsEditor)).OnInspectorGUI();

            GUILayout.Space(8);

            BuilderGUILayout.DrawLink("Need Help? Visit our documentation", "https://www.mindport.co/vr-builder-tutorials/text-to-speech-audio", 0);
        }

        ~TextToSpeechSectionProvider()
        {
            if (EditorUtility.IsDirty(TextToSpeechSettings.Instance))
            {
                TextToSpeechSettings.Instance.Save();
            }
        }
    }
}
