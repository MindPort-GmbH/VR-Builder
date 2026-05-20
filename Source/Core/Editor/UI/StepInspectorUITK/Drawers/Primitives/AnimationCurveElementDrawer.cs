using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Primitives
{
    [DefaultProcessElementDrawer(typeof(AnimationCurve))]
    internal class AnimationCurveElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            AnimationCurve oldValue = value as AnimationCurve ?? AnimationCurve.Linear(0f, 0f, 1f, 1f);

            CurveField field = new CurveField(label?.text)
            {
                value = oldValue,
                tooltip = label?.tooltip
            };
            field.AddToClassList("vrb-field");
            field.AddToClassList("vrb-field--curve");

            field.RegisterCallback<ChangeEvent<AnimationCurve>>(evt =>
            {
                AnimationCurve committedNew = evt.newValue;
                AnimationCurve committedOld = oldValue;
                if (ReferenceEquals(committedNew, committedOld)) return;

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
