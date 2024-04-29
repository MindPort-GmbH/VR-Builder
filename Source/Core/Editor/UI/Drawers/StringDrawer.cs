// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Editor.UI.Drawers
{
    /// <summary>
    /// Process drawer for string members.
    /// </summary>
    [DefaultProcessDrawer(typeof(string))]
    internal class StringDrawer : AbstractDrawer
    {
        public bool IsMultiLine { get; set; }

        /// <inheritdoc />
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            string stringValue = (string)currentValue;
            string newValue;
            if (IsMultiLine)
            {
                var labelRect = new Rect(rect.xMin, rect.yMin, rect.width, EditorGUIUtility.singleLineHeight);
                float lineHeight = EditorGUIUtility.singleLineHeight * (("" + stringValue).Count(c => c == '\n') + 1);
                var textRect = new Rect(rect.xMin, rect.yMin + labelRect.height + 2f, rect.width, rect.height - labelRect.height - 3f + lineHeight);
                rect = new Rect(rect.x, rect.y, rect.width, rect.height + lineHeight);

                EditorGUI.LabelField(labelRect, label);
                newValue = EditorGUI.TextArea(textRect, stringValue);
            }
            else
            {
                rect.height = EditorDrawingHelper.SingleLineHeight;
                newValue = EditorGUI.TextField(rect, label, stringValue);
            }

            if (stringValue != newValue)
            {
                ChangeValue(() => newValue, () => stringValue, changeValueCallback);
            }

            return rect;
        }
    }
}
