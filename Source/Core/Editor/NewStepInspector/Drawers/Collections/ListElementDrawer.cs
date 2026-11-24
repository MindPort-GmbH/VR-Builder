// Copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// UIToolkit drawer for IList values.
    /// Uses a Foldout with items inside for nested data display.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(IList))]
    public class ListElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            IList list = (IList)currentValue;

            Foldout foldout = new Foldout
            {
                text = string.IsNullOrEmpty(label) ? "List" : label,
                value = true
            };
            foldout.style.marginLeft = 8;

            if (list == null || list.Count == 0)
            {
                foldout.Add(new Label("(empty)"));
                return foldout;
            }

            for (int i = 0; i < list.Count; i++)
            {
                int closuredIndex = i;
                object item = list[closuredIndex];

                if (item == null)
                {
                    foldout.Add(new Label($"[{closuredIndex}] null"));
                    continue;
                }

                Type itemType = item.GetType();
                IElementDrawer itemDrawer = ElementDrawerLocator.GetDrawerForValue(item, itemType);

                if (itemDrawer == null)
                {
                    foldout.Add(new Label($"[{closuredIndex}] {itemType.Name} (no drawer)"));
                    continue;
                }

                string itemLabel = itemDrawer.GetLabel(item, itemType);
                if (string.IsNullOrEmpty(itemLabel))
                {
                    itemLabel = $"[{closuredIndex}]";
                }

                VisualElement itemElement = itemDrawer.CreateElement(item, (newValue) =>
                {
                    list[closuredIndex] = newValue;
                    changeValueCallback(currentValue);
                }, itemLabel);

                if (itemElement != null)
                {
                    foldout.Add(itemElement);
                }
            }

            return foldout;
        }
    }
}
