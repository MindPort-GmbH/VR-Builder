// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers
{
    /// <summary>
    /// Default drawer for <see cref="IList"/> values. Renders one child drawer per entry
    /// in a vertical stack. Reorder UI / add+remove buttons land in Phase 6.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(IList))]
    internal class ListElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            VisualElement root = new VisualElement();
            root.AddToClassList("vrb-list");

            if (value == null)
            {
                Label nullLabel = new Label(label?.text ?? "(empty list)") { tooltip = label?.tooltip };
                nullLabel.AddToClassList("vrb-list__null");
                root.Add(nullLabel);
                return root;
            }

            if (label != null && label != GUIContent.none && (label.image != null || string.IsNullOrEmpty(label.text) == false))
            {
                Label header = new Label(label.text) { tooltip = label.tooltip };
                header.AddToClassList("vrb-list__header");
                root.Add(header);
            }

            IList list = (IList)value;
            Type entryDeclaredType = ReflectionUtils.GetEntryType(list);

            for (int i = 0; i < list.Count; i++)
            {
                int closuredIndex = i;
                object entry = list[closuredIndex];

                IElementDrawer entryDrawer = ElementDrawerLocator.GetDrawerForValue(entry, entryDeclaredType);
                if (entryDrawer == null)
                {
                    continue;
                }

                Action<object> entryChanged = newValue =>
                {
                    if (closuredIndex >= list.Count)
                    {
                        ReflectionUtils.InsertIntoList(ref list, Math.Min(closuredIndex, list.Count), newValue);
                    }
                    else
                    {
                        list[closuredIndex] = newValue;
                    }

                    if (newValue == null)
                    {
                        ReflectionUtils.RemoveFromList(ref list, closuredIndex);
                    }

                    changeCallback(list);
                };

                GUIContent entryLabel = entryDrawer.GetLabel(entry, entryDeclaredType);
                VisualElement row = entryDrawer.CreateElement(entry, entryChanged, entryLabel);
                if (row != null)
                {
                    row.AddToClassList("vrb-list__item");
                    root.Add(row);
                }
            }

            return root;
        }
    }
}
