using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using VRBuilder.Editor.UI.Drawers;
using VRBuilder.TextToSpeech.Audio;

namespace VRBuilder.Editor.Core.UI.Drawers
{
    ///<author email="a.schaub@lefx.de">Aron Schaub</author>
    [DefaultProcessDrawer(typeof(LocalizedTextToSpeechAudio))]
    public class LocalizedTextToSpeechAudioDrawer : ObjectDrawer
    {
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            float oldRectX = rect.x;
            Vector2 size;

            EditorGUILayout.BeginHorizontal();

            if (currentValue is LocalizedTextToSpeechAudio currentObject)
            {
                string fieldName = string.IsNullOrEmpty(currentObject.LocalizationTable)
                    ? "(None)"
                    : currentObject.LocalizationTable;

                size = GUI.skin.label.CalcSize(new GUIContent(nameof(currentObject.LocalizationTable)));

                EditorGUI.LabelField(rect, nameof(currentObject.LocalizationTable));
                size.x += EditorGUIUtility.singleLineHeight; //intentionally singleLineHeight on a width, to have the same gap
                rect.x += size.x;
                rect.width -= size.x;

                if (EditorGUI.DropdownButton(rect, new GUIContent(fieldName), FocusType.Passive))
                {
                    void HandleItemClicked(object parameter)
                    {
                        if (parameter is string selectedAssetPath)
                        {
                            currentObject.LocalizationTable = Path.GetFileNameWithoutExtension(selectedAssetPath);
                        }
                    }
                    
                    // LocalizationEditorSettings.GetStringTableCollections()
                    
                    // GetTableTask ??= LocalizationSettings.StringDatabase.GetAllTables(); // this fails in editor

                    IEnumerable<string> stringTableCollections = AssetDatabase.FindAssets("t:" + nameof(StringTableCollection)).Select(AssetDatabase.GUIDToAssetPath);

                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("None"), false, HandleItemClicked, "");
                    foreach (string stringTable in stringTableCollections)
                    {
                        menu.AddItem(new GUIContent(stringTable), false, HandleItemClicked, stringTable);
                    }

                    menu.DropDown(rect);
                }
            }
            else
                size = new Vector2();

            rect.x = oldRectX;
            rect.width += size.x;

            EditorGUILayout.EndHorizontal();
            rect.y += EditorGUIUtility.singleLineHeight; // DropdownButton
            var draw = base.Draw(rect, currentValue, changeValueCallback, GUIContent.none);
            draw.height += EditorGUIUtility.singleLineHeight; // DropdownButton
            return draw;
        }

        protected override IEnumerable<MemberInfo> GetMembersToDraw(object value)
        {
            var locaTableSet = false;
            if (value is LocalizedTextToSpeechAudio currentObject)
            {
                locaTableSet = string.IsNullOrEmpty(currentObject.LocalizationTable);
            }

            return base.GetMembersToDraw(value).Where(mi =>
            {
                switch (locaTableSet)
                {
                    case false when mi.Name.Contains("Text"): //key or text
                    case true when mi.Name.Contains("Key"):
                        return false;
                    default:
                        return !mi.Name.Contains("LocalizationTable");
                }
            });
        }
    }
}