// Copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// UIToolkit drawer for bool values. Returns a Toggle.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(bool))]
    public class BoolElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            bool value = (bool)currentValue;
            Toggle toggle = new Toggle(label ?? string.Empty) { value = value };

            toggle.RegisterValueChangedCallback(evt =>
            {
                if (value != evt.newValue)
                {
                    bool oldValue = value;
                    value = evt.newValue;
                    ChangeValue(() => evt.newValue, () => oldValue, changeValueCallback);
                }
            });

            return toggle;
        }
    }
}
