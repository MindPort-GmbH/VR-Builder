// Copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    [DefaultProcessElementDrawer(typeof(Color))]
    public class ColorElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            Color value = (Color)currentValue;
            ColorField field = new ColorField(label ?? string.Empty) { value = value };

            field.RegisterValueChangedCallback(evt =>
            {
                Color oldValue = value;
                value = evt.newValue;
                ChangeValue(() => evt.newValue, () => oldValue, changeValueCallback);
            });

            return field;
        }
    }
}
