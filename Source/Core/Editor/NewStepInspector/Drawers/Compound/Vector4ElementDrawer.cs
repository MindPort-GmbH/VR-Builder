// Copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    [DefaultProcessElementDrawer(typeof(Vector4))]
    public class Vector4ElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            Vector4 value = (Vector4)currentValue;
            Vector4Field field = new Vector4Field(label ?? string.Empty) { value = value };

            field.RegisterValueChangedCallback(evt =>
            {
                Vector4 oldValue = value;
                value = evt.newValue;
                ChangeValue(() => evt.newValue, () => oldValue, changeValueCallback);
            });

            return field;
        }
    }
}
