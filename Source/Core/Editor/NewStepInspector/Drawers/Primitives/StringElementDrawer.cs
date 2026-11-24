// Copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// UIToolkit drawer for string values. Returns a TextField.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(string))]
    public class StringElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            string value = currentValue as string ?? string.Empty;
            TextField field = new TextField(label ?? string.Empty) { value = value };

            field.RegisterValueChangedCallback(evt =>
            {
                if (value != evt.newValue)
                {
                    string oldValue = value;
                    value = evt.newValue;
                    ChangeValue(() => evt.newValue, () => oldValue, changeValueCallback);
                }
            });

            return field;
        }
    }
}
