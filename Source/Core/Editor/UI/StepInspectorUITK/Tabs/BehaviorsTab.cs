// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs
{
    internal sealed class BehaviorsTab : IStepInspectorPanel
    {
        public string Id => PanelIds.Behaviors;
        public GUIContent Label { get; } = new GUIContent("Behaviors");

        public VisualElement BuildContent(IStepData step, IElementDrawerContext ctx)
        {
            Label placeholder = new Label("BehaviorsTab — pending Phase 5");
            placeholder.AddToClassList("vrb-panel-stub");
            placeholder.AddToClassList("vrb-panel-stub--behaviors");
            return placeholder;
        }

        public void Refresh() { }
        public void Dispose() { }
    }
}
