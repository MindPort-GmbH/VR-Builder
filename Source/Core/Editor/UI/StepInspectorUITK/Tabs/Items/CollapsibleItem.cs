// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items
{
    /// <summary>
    /// Behavior / transition / condition row used by every tab.
    /// Layout: [grip] [caret] [title (flex-grow)] [delete] above a collapsible body.
    /// </summary>
    internal sealed class CollapsibleItem : VisualElement
    {
        public bool IsExpanded { get; private set; }
        public VisualElement Body { get; }

        public CollapsibleItem(string title, Action onDelete, string gripTooltip, string deleteTooltip, bool startExpanded = true)
        {
            IsExpanded = startExpanded;
            AddToClassList("vrb-item");

            VisualElement header = new VisualElement();
            header.AddToClassList("vrb-item__header");
            header.style.flexDirection = FlexDirection.Row;

            VisualElement grip = new VisualElement();
            grip.AddToClassList("vrb-grip");
            grip.tooltip = gripTooltip;
            header.Add(grip);

            Button caret = new Button { text = startExpanded ? "▼" : "▶" };
            caret.AddToClassList("vrb-item__caret");
            header.Add(caret);

            Label titleLabel = new Label(title ?? string.Empty);
            titleLabel.AddToClassList("vrb-item__title");
            titleLabel.style.flexGrow = 1f;
            header.Add(titleLabel);

            Button deleteButton = new Button(() => onDelete?.Invoke())
            {
                text = "✕",
                tooltip = deleteTooltip
            };
            deleteButton.AddToClassList("vrb-item__delete");
            header.Add(deleteButton);

            Add(header);

            Body = new VisualElement();
            Body.AddToClassList("vrb-item__body");
            Body.style.display = startExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            Add(Body);

            // Clicking the caret button toggles. Clicking the title label is a nice extra.
            caret.clicked += Toggle;
            titleLabel.RegisterCallback<ClickEvent>(_ => Toggle());
        }

        public void Toggle()
        {
            IsExpanded = !IsExpanded;
            Body.style.display = IsExpanded ? DisplayStyle.Flex : DisplayStyle.None;

            Button caret = this.Q<Button>(className: "vrb-item__caret");
            if (caret != null)
            {
                caret.text = IsExpanded ? "▼" : "▶";
            }
        }
    }
}
