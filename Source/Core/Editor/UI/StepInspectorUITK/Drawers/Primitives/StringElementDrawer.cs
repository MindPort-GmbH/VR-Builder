// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Primitives
{
    [DefaultProcessElementDrawer(typeof(string))]
    internal class StringElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            string oldValue = value as string ?? string.Empty;

            // isDelayed: commit on Enter / focus loss to avoid one undo entry per keystroke.
            TextField field = new TextField(label?.text)
            {
                value = oldValue,
                tooltip = label?.tooltip,
                isDelayed = true,
                multiline = false
            };
            field.AddToClassList("vrb-field");
            field.AddToClassList("vrb-field--string");

            field.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                string committedNew = evt.newValue ?? string.Empty;
                string committedOld = oldValue;
                if (committedNew == committedOld)
                {
                    return;
                }

                ChangeValue(
                    getNewValueCallback: () => committedNew,
                    getOldValueCallback: () => committedOld,
                    assignValueCallback: changeCallback);

                oldValue = committedNew;
            });

            return field;
        }
    }
}
