// Copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    [DefaultProcessElementDrawer(typeof(Vector2))]
    public class Vector2ElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            Vector2 value = (Vector2)currentValue;
            Vector2Field field = new Vector2Field(label ?? string.Empty) { value = value };

            field.RegisterValueChangedCallback(evt =>
            {
                Vector2 oldValue = value;
                value = evt.newValue;
                ChangeValue(() => evt.newValue, () => oldValue, changeValueCallback);
            });

            return field;
        }
    }
}
