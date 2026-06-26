using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Primitives
{
    [DefaultProcessElementDrawer(typeof(int))]
    internal class IntElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            int oldValue = value is int i ? i : 0;

            IntegerField field = new IntegerField(label?.text)
            {
                value = oldValue,
                tooltip = label?.tooltip,
                isDelayed = true
            };
            field.AddToClassList("vrb-field");
            field.AddToClassList("vrb-field--int");

            field.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                int committedNew = evt.newValue;
                int committedOld = oldValue;
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
