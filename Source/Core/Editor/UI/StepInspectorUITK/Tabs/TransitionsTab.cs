// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs
{
    internal sealed class TransitionsTab : IStepInspectorPanel
    {
        public string Id => PanelIds.Transitions;
        public GUIContent Label { get; } = new GUIContent("Transitions");

        public VisualElement BuildContent(IStepData step, IElementDrawerContext ctx)
        {
            Label placeholder = new Label("TransitionsTab — pending Phase 5");
            placeholder.AddToClassList("vrb-panel-stub");
            placeholder.AddToClassList("vrb-panel-stub--transitions");
            return placeholder;
        }

        public void Refresh() { }
        public void Dispose() { }
    }
}
