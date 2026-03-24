// Copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// UIToolkit drawer for Enum values. Returns an EnumField.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(Enum))]
    public class EnumElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            Enum value = (Enum)currentValue;
            EnumField field = new EnumField(label ?? string.Empty, value);

            field.RegisterValueChangedCallback(evt =>
            {
                if (!Equals(value, evt.newValue))
                {
                    Enum oldValue = value;
                    value = evt.newValue;
                    ChangeValue(() => evt.newValue, () => oldValue, changeValueCallback);
                }
            });

            return field;
        }
    }
}
