// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.DragDrop;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items
{
    /// <summary>
    /// Behavior / transition / condition row used by every tab.
    /// Header layout: [grip] [caret] [title (flex-grow)] [extra action buttons...] [delete]
    /// Body underneath is collapsible.
    /// </summary>
    internal sealed class CollapsibleItem : VisualElement
    {
        public readonly struct HeaderAction
        {
            public readonly string Glyph;
            public readonly string Tooltip;
            public readonly Action Callback;
            public readonly string CssModifier;
            public readonly bool Visible;

            public HeaderAction(string glyph, string tooltip, Action callback, string cssModifier = null, bool visible = true)
            {
                Glyph = glyph;
                Tooltip = tooltip;
                Callback = callback;
                CssModifier = cssModifier;
                Visible = visible;
            }
        }

        public bool IsExpanded { get; private set; }
        public VisualElement Body { get; }
        public VisualElement Grip { get; }
        public VisualElement Header { get; }

        private readonly object stateKey;

        /// <summary>
        /// Builds a row with the standard grip + caret + title + a trailing delete button.
        /// </summary>
        public CollapsibleItem(string title, Action onDelete, string gripTooltip, string deleteTooltip,
            bool startExpanded = true, object stateKey = null)
            : this(title, gripTooltip, deleteTooltip, onDelete, extraActions: null, startExpanded, stateKey)
        {
        }

        public CollapsibleItem(
            string title,
            string gripTooltip,
            string deleteTooltip,
            Action onDelete,
            IEnumerable<HeaderAction> extraActions,
            bool startExpanded = true,
            object stateKey = null)
        {
            this.stateKey = stateKey;

            // Restore the user's last expand/collapse choice for this entity if any.
            // Falls back to startExpanded when this key has never been toggled.
            bool effectiveExpanded = FoldoutState.Get(stateKey) ?? startExpanded;
            IsExpanded = effectiveExpanded;

            AddToClassList("vrb-item");

            VisualElement header = new VisualElement();
            header.AddToClassList("vrb-item__header");
            header.style.flexDirection = FlexDirection.Row;
            Header = header;

            // The drag handle's two horizontal bars are drawn by USS borders on an empty
            // element — matches the look of Unity's IMGUI ReorderableList handle.
            VisualElement grip = new VisualElement();
            grip.AddToClassList("vrb-grip");
            grip.tooltip = gripTooltip;
            header.Add(grip);
            Grip = grip;

            Button caret = new Button { text = IsExpanded ? Icons.Caret : Icons.CaretCollapsed };
            caret.AddToClassList("vrb-item__caret");
            header.Add(caret);

            Label titleLabel = new Label(title ?? string.Empty);
            titleLabel.AddToClassList("vrb-item__title");
            titleLabel.style.flexGrow = 1f;
            header.Add(titleLabel);

            if (extraActions != null)
            {
                foreach (HeaderAction action in extraActions)
                {
                    if (!action.Visible || action.Callback == null)
                    {
                        continue;
                    }

                    Button actionButton = new Button(() => action.Callback())
                    {
                        text = action.Glyph,
                        tooltip = action.Tooltip
                    };
                    actionButton.AddToClassList("vrb-item__action");
                    if (string.IsNullOrEmpty(action.CssModifier) == false)
                    {
                        actionButton.AddToClassList(action.CssModifier);
                    }
                    header.Add(actionButton);
                }
            }

            if (onDelete != null)
            {
                Button deleteButton = new Button(() => onDelete())
                {
                    text = Icons.Delete,
                    tooltip = deleteTooltip
                };
                deleteButton.AddToClassList("vrb-item__delete");
                header.Add(deleteButton);
            }

            Add(header);

            Body = new VisualElement();
            Body.AddToClassList("vrb-item__body");
            Body.style.display = IsExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            Add(Body);

            caret.clicked += () =>
            {
                // Drag may have just finished — UITK can still synthesize a click on the
                // caret in that case. Don't toggle the foldout under those circumstances.
                if (DragSession.ConsumeSuppressClick()) return;
                Toggle();
            };
            titleLabel.RegisterCallback<ClickEvent>(_ =>
            {
                if (DragSession.ConsumeSuppressClick()) return;
                Toggle();
            });
        }

        public void Toggle()
        {
            IsExpanded = !IsExpanded;
            Body.style.display = IsExpanded ? DisplayStyle.Flex : DisplayStyle.None;

            Button caret = this.Q<Button>(className: "vrb-item__caret");
            if (caret != null)
            {
                caret.text = IsExpanded ? Icons.Caret : Icons.CaretCollapsed;
            }

            // Persist so a panel rebuild (e.g. after reorder) keeps the same shape.
            FoldoutState.Set(stateKey, IsExpanded);
        }
    }
}
