using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Primitives
{
    [DefaultProcessElementDrawer(typeof(Vector3))]
    internal class Vector3ElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            Vector3 oldValue = value is Vector3 v ? v : Vector3.zero;

            Vector3Field field = new Vector3Field(label?.text)
            {
                value = oldValue,
                tooltip = label?.tooltip
            };
            field.AddToClassList("vrb-field");
            field.AddToClassList("vrb-field--vector3");

            field.RegisterCallback<ChangeEvent<Vector3>>(evt =>
            {
                Vector3 committedNew = evt.newValue;
                Vector3 committedOld = oldValue;
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
