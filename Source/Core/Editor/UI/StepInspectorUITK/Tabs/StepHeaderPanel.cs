// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs
{
    internal sealed class StepHeaderPanel : IStepInspectorPanel
    {
        public string Id => PanelIds.Header;
        public GUIContent Label { get; } = new GUIContent("Step");

        public VisualElement BuildContent(IStepData step, IElementDrawerContext ctx)
        {
            // Real layout (name field, description, breadcrumb) lands in Phase 5 + the
            // Phase 8 step-name placement decision.
            Label placeholder = new Label("StepHeaderPanel — pending Phase 5");
            placeholder.AddToClassList("vrb-panel-stub");
            placeholder.AddToClassList("vrb-panel-stub--header");
            return placeholder;
        }

        public void Refresh() { }
        public void Dispose() { }
    }
}
