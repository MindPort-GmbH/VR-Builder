using System;
using System.IO;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using VRBuilder.Core.Editor.UI.Drawers;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Draws a drop-down for localization tables.
    /// </summary>
    ///<author email="a.schaub@lefx.de">Aron Schaub</author>
    public class LocalizationTableDrawer : ObjectDrawer
    {
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            float oldRectX = rect.x;
            Vector2 size;

            EditorGUILayout.BeginHorizontal();

            if (currentValue is string newValue)
            {
                string fieldName = string.IsNullOrEmpty(newValue)
                    ? "<None>"
                    : newValue;

                size = GUI.skin.label.CalcSize(new GUIContent("Localization Table"));

                EditorGUI.LabelField(rect, "Localization Table");
                size.x += EditorGUIUtility.singleLineHeight; //intentionally singleLineHeight on a width, to have the same gap
                rect.x += size.x;
                rect.width -= size.x;

                if (EditorGUI.DropdownButton(rect, new GUIContent(fieldName), FocusType.Passive))
                {
                    void HandleItemClicked(object parameter)
                    {
                        if (parameter is string stringTableName)
                        {
                            newValue = Path.GetFileNameWithoutExtension(stringTableName);
                            ChangeValue(() => newValue, () => currentValue, changeValueCallback);
                        }
                    }

                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("None"), false, HandleItemClicked, "");
                    foreach (StringTableCollection stringTable in LocalizationEditorSettings.GetStringTableCollections())
                    {
                        menu.AddItem(new GUIContent($"{stringTable.Group}/{stringTable.TableCollectionName}"), false, HandleItemClicked, stringTable.name);
                    }

                    menu.DropDown(rect);
                }
            }
            else
            {
                size = new Vector2();
            }

            rect.x = oldRectX;
            rect.width += size.x;

            EditorGUILayout.EndHorizontal();
            return rect;
        }
    }
}