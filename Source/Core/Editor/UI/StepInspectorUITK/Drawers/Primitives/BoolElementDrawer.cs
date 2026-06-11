using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Primitives
{
    [DefaultProcessElementDrawer(typeof(bool))]
    internal class BoolElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            bool oldValue = value is bool b && b;

            // Use the toggle's own `text` (rendered right next to the checkmark) instead of the
            // BaseField label, which would sit in the fixed-width left label column far from the box.
            Toggle field = new Toggle
            {
                value = oldValue,
                text = label?.text,
                tooltip = label?.tooltip
            };
            field.AddToClassList("vrb-field");
            field.AddToClassList("vrb-field--bool");

            field.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                bool committedNew = evt.newValue;
                bool committedOld = oldValue;
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
