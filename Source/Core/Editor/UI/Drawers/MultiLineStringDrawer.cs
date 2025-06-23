// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Process drawer for string members.
    /// </summary>
    internal class MultiLineStringDrawer : AbstractDrawer
    {
        /// <inheritdoc />
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            string stringValue = (string)currentValue;
            string newValue;

            // Calculate the height based on the wrapped text
            GUIStyle style = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };
            float textHeight = style.CalcHeight(new GUIContent(stringValue), rect.width);

            // Define the label and text areas
            var labelRect = new Rect(rect.xMin, rect.yMin, rect.width, EditorGUIUtility.singleLineHeight);
            var textRect = new Rect(rect.xMin, rect.yMin + labelRect.height + 2f, rect.width, textHeight);

            // Adjust the overall rect height to accommodate the text
            rect = new Rect(rect.x, rect.y, rect.width, rect.height + textHeight);

            // Draw the label and text area
            EditorGUI.LabelField(labelRect, label);
            newValue = EditorGUI.TextArea(textRect, stringValue, style);

            if (stringValue != newValue)
            {
                ChangeValue(() => newValue, () => stringValue, changeValueCallback);
            }

            return rect;
        }
    }
}
