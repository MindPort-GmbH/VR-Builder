// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Primitives
{
    [DefaultProcessElementDrawer(typeof(float))]
    internal class FloatElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            float oldValue = value is float f ? f : 0f;

            // isDelayed: commit on Enter / focus loss so dragging the spinner doesn't flood the undo stack.
            FloatField field = new FloatField(label?.text)
            {
                value = oldValue,
                tooltip = label?.tooltip,
                isDelayed = true
            };
            field.AddToClassList("vrb-field");
            field.AddToClassList("vrb-field--float");

            field.RegisterCallback<ChangeEvent<float>>(evt =>
            {
                float committedNew = evt.newValue;
                float committedOld = oldValue;
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
