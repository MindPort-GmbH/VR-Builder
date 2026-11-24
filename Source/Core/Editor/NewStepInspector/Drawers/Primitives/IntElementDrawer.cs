// Copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// UIToolkit drawer for int values. Returns an IntegerField.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(int))]
    public class IntElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            int value = (int)currentValue;
            IntegerField field = new IntegerField(label ?? string.Empty) { value = value };

            field.RegisterValueChangedCallback(evt =>
            {
                if (value != evt.newValue)
                {
                    int oldValue = value;
                    value = evt.newValue;
                    ChangeValue(() => evt.newValue, () => oldValue, changeValueCallback);
                }
            });

            return field;
        }
    }
}
