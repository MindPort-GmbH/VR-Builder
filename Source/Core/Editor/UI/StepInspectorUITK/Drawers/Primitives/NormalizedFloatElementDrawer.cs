// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Primitives
{
    /// <summary>
    /// 0-1 slider variant of <see cref="FloatElementDrawer"/>. Picked up via
    /// <c>[UsesSpecificProcessDrawer("NormalizedFloatDrawer")]</c> on a float member.
    /// Slider drags commit a single undo entry (PointerDown captures the start value;
    /// live drag updates the data without recording undo; PointerUp commits the final).
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

            slider.RegisterCallback<PointerDownEvent>(evt =>
            {
                // Ignore PointerDown on the inline input field — only drags on the slider
                // track / thumb should batch into a single undo entry.
                if (IsInsideInputField(evt.target as VisualElement))
                {
                    return;
                }

                dragging = true;
                dragStartValue = committedValue;
            });

            slider.RegisterCallback<PointerUpEvent>(_ => CommitDragIfActive());
            slider.RegisterCallback<PointerCaptureOutEvent>(_ => CommitDragIfActive());

            slider.RegisterCallback<ChangeEvent<float>>(evt =>
            {
                float live = Mathf.Clamp01(evt.newValue);

                if (dragging)
                {
                    // Live drag: update the underlying data so the visual tracks the thumb,
                    // but don't push an undo entry per pixel.
                    changeCallback?.Invoke(live);
                    return;
                }

                if (Mathf.Approximately(live, committedValue))
                {
                    return;
                }

                // Came from the inline input field (or programmatic set) — commit immediately.
                float captureNew = live;
                float captureOld = committedValue;
                ChangeValue(
                    getNewValueCallback: () => captureNew,
                    getOldValueCallback: () => captureOld,
                    assignValueCallback: changeCallback);

                committedValue = live;
            });

            return slider;

            void CommitDragIfActive()
            {
                if (!dragging)
                {
                    return;
                }

                dragging = false;
                float final = Mathf.Clamp01(slider.value);

                if (Mathf.Approximately(final, dragStartValue))
                {
                    committedValue = final;
                    return;
                }

                float captureNew = final;
                float captureOld = dragStartValue;
                ChangeValue(
                    getNewValueCallback: () => captureNew,
                    getOldValueCallback: () => captureOld,
                    assignValueCallback: changeCallback);

                committedValue = final;
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
