using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Primitives
{
    [DefaultProcessElementDrawer(typeof(Color32))]
    internal class UnityColor32ElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            Color32 oldValue = value is Color32 c ? c : new Color32(255, 255, 255, 255);

            ColorField field = new ColorField(label?.text)
            {
                value = (Color)oldValue,
                tooltip = label?.tooltip
            };
            field.AddToClassList("vrb-field");
            field.AddToClassList("vrb-field--color32");

            field.RegisterCallback<ChangeEvent<Color>>(evt =>
            {
                Color32 committedNew = evt.newValue;
                Color32 committedOld = oldValue;
                if (committedNew.r == committedOld.r && committedNew.g == committedOld.g
                    && committedNew.b == committedOld.b && committedNew.a == committedOld.a)
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
