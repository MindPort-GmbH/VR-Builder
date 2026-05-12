// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    /// <summary>
    /// Header + body container shared by every panel window so panels look identical
    /// regardless of which window is hosting them.
    /// </summary>
    internal sealed class PanelHost : VisualElement
    {
        public string PanelId { get; }

        public PanelHost(string panelId, string title, VisualElement body)
        {
            PanelId = panelId;
            AddToClassList("vrb-panel-host");

            VisualElement header = new VisualElement { name = "vrb-panel-host__header" };
            header.AddToClassList("vrb-panel-host__header");

            Label titleLabel = new Label(title ?? string.Empty) { name = "vrb-panel-host__title" };
            titleLabel.AddToClassList("vrb-panel-host__title");
            header.Add(titleLabel);

            VisualElement bodyContainer = new VisualElement { name = "vrb-panel-host__body" };
            bodyContainer.AddToClassList("vrb-panel-host__body");
            if (body != null)
            {
                bodyContainer.Add(body);
            }

            Add(header);
            Add(bodyContainer);
        }
    }
}
