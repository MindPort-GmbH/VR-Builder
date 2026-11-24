// Copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    [DefaultProcessElementDrawer(typeof(Vector3))]
    public class Vector3ElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            Vector3 value = (Vector3)currentValue;
            Vector3Field field = new Vector3Field(label ?? string.Empty) { value = value };

            field.RegisterValueChangedCallback(evt =>
            {
                Vector3 oldValue = value;
                value = evt.newValue;
                ChangeValue(() => evt.newValue, () => oldValue, changeValueCallback);
            });

            return field;
        }
    }
}
