// Copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// UIToolkit drawer for <see cref="AnimationCurve"/>.
    /// Uses <see cref="CurveField"/> which provides an interactive curve editor.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(AnimationCurve))]
    public class AnimationCurveElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            AnimationCurve curve = (AnimationCurve)currentValue ?? new AnimationCurve();

            CurveField field = new CurveField(label ?? string.Empty);
            field.value = curve;

            field.RegisterValueChangedCallback(evt =>
            {
                AnimationCurve oldCurve = curve;
                curve = evt.newValue;
                ChangeValue(() => evt.newValue, () => oldCurve, changeValueCallback);
            });

            return field;
        }
    }
}
