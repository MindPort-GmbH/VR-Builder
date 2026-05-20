using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Primitives
{
    /// <summary>
    /// 0-1 slider variant of <see cref="FloatElementDrawer"/>. Picked up via
    /// <c>[UsesSpecificProcessDrawer("NormalizedFloatDrawer")]</c> on a float member.
    /// Slider drags update live and commit one undo entry on pointer release.
    /// Typing into the inline input field commits immediately.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(NormalizedFloat))]
    internal class NormalizedFloatDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            float committedValue = value is float f ? Mathf.Clamp01(f) : 0f;

            Slider slider = new Slider(label?.text, 0f, 1f)
            {
                value = committedValue,
                tooltip = label?.tooltip,
                showInputField = true
            };
            slider.AddToClassList("vrb-field");
            slider.AddToClassList("vrb-field--normalized-float");
            slider.style.flexGrow = 1f;

            bool dragging = false;
            float dragStartValue = committedValue;
            int activePointerId = PointerId.invalidPointerId;

            slider.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (dragging || IsInsideInputField(evt.target as VisualElement))
                {
                    return;
                }

                dragging = true;
                activePointerId = evt.pointerId;
                dragStartValue = committedValue;
            }, TrickleDown.TrickleDown);

            slider.RegisterCallback<PointerUpEvent>(evt => CommitDragIfActive(evt.pointerId), TrickleDown.TrickleDown);
            slider.RegisterCallback<PointerCancelEvent>(evt => CommitDragIfActive(evt.pointerId), TrickleDown.TrickleDown);

            slider.RegisterCallback<ChangeEvent<float>>(evt =>
            {
                float newValue = Mathf.Clamp01(evt.newValue);

                if (dragging)
                {
                    changeCallback?.Invoke(newValue);
                    return;
                }

                float oldValue = committedValue;
                if (Mathf.Approximately(newValue, oldValue))
                {
                    return;
                }

                ChangeValue(
                    getNewValueCallback: () => newValue,
                    getOldValueCallback: () => oldValue,
                    assignValueCallback: changeCallback);

                committedValue = newValue;
            });

            return slider;

            void CommitDragIfActive(int pointerId)
            {
                if (!dragging || pointerId != activePointerId)
                {
                    return;
                }

                dragging = false;
                activePointerId = PointerId.invalidPointerId;

                float finalValue = Mathf.Clamp01(slider.value);
                if (Mathf.Approximately(finalValue, dragStartValue))
                {
                    committedValue = finalValue;
                    return;
                }

                float oldValue = dragStartValue;
                ChangeValue(
                    getNewValueCallback: () => finalValue,
                    getOldValueCallback: () => oldValue,
                    assignValueCallback: changeCallback);

                committedValue = finalValue;
            }
        }

        private static bool IsInsideInputField(VisualElement element)
        {
            while (element != null)
            {
                if (element is TextField || element is FloatField || element is IntegerField)
                {
                    return true;
                }

                element = element.parent;
            }

            return false;
        }
    }

    // Marker type — never instantiated. Exists so the locator's name-based
    // resolution of [UsesSpecificProcessDrawer("NormalizedFloatDrawer")] finds it.
    internal sealed class NormalizedFloat { private NormalizedFloat() { } }
}
