// Copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// UIToolkit drawer for float values. Returns a FloatField.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(float))]
    public class FloatElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            float value = (float)currentValue;
            FloatField field = new FloatField(label ?? string.Empty) { value = value };

            field.RegisterValueChangedCallback(evt =>
            {
                if (!Mathf.Approximately(value, evt.newValue))
                {
                    float oldValue = value;
                    value = evt.newValue;
                    ChangeValue(() => evt.newValue, () => oldValue, changeValueCallback);
                }
            });

            return field;
        }
    }
}
