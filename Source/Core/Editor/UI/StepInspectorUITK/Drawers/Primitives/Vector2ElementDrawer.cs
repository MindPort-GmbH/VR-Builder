// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Primitives
{
    [DefaultProcessElementDrawer(typeof(Vector2))]
    internal class Vector2ElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            Vector2 oldValue = value is Vector2 v ? v : Vector2.zero;

            Vector2Field field = new Vector2Field(label?.text)
            {
                value = oldValue,
                tooltip = label?.tooltip
            };
            field.AddToClassList("vrb-field");
            field.AddToClassList("vrb-field--vector2");

            field.RegisterCallback<ChangeEvent<Vector2>>(evt =>
            {
                Vector2 committedNew = evt.newValue;
                Vector2 committedOld = oldValue;
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
