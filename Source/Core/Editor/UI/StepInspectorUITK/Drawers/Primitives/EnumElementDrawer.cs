using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Primitives
{
    [DefaultProcessElementDrawer(typeof(Enum))]
    internal class EnumElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            Enum enumValue = value as Enum;
            if (enumValue == null)
            {
                // EnumField needs a concrete enum instance for its initial value.
                Label fallback = new Label((label?.text ?? "Enum") + ": <null>")
                {
                    tooltip = label?.tooltip
                };
                fallback.AddToClassList("vrb-field");
                fallback.AddToClassList("vrb-field--enum-null");
                return fallback;
            }

            Enum oldValue = enumValue;

            EnumField field = new EnumField(label?.text, enumValue)
            {
                tooltip = label?.tooltip
            };
            field.AddToClassList("vrb-field");
            field.AddToClassList("vrb-field--enum");

            field.RegisterCallback<ChangeEvent<Enum>>(evt =>
            {
                Enum committedNew = evt.newValue;
                Enum committedOld = oldValue;
                if (Equals(committedNew, committedOld))
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
