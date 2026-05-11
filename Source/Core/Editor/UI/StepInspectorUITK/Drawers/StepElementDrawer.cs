// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers
{
    /// <summary>
    /// Owns the entire Step Inspector content for a <see cref="Step.EntityData"/>:
    /// the header, the behaviors panel, the transitions panel, and the unlocked-objects panel.
    /// Custom step types can subclass this to swap or rearrange panels without changing window code.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(Step.EntityData))]
    public class StepElementDrawer : ObjectElementDrawer
    {
        private readonly Dictionary<string, IStepInspectorPanel> panelCache = new Dictionary<string, IStepInspectorPanel>();
        private Step.EntityData lastStep;

        /// <summary>
        /// The default render: stack all panels vertically. The shell window in Phase 4
        /// instead calls <see cref="BuildPanel"/> per id so it can host each panel separately.
        /// </summary>
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            VisualElement root = new VisualElement { name = "step-inspector-root" };
            root.AddToClassList("vrb-step");

            Step.EntityData step = value as Step.EntityData;
            if (step == null)
            {
                root.Add(new Label("(no step selected)"));
                return root;
            }

            foreach (string panelId in PanelIds.AllInOrder)
            {
                VisualElement panelContent = BuildPanel(panelId, step, changeCallback);
                if (panelContent != null)
                {
                    root.Add(panelContent);
                }
            }

            return root;
        }

        /// <summary>
        /// Builds the <see cref="VisualElement"/> for one panel. Reused by the shell window
        /// (Phase 4) and by the per-panel detached windows.
        /// </summary>
        public VisualElement BuildPanel(string panelId, Step.EntityData step, Action<object> changeCallback)
        {
            if (step == null)
            {
                return null;
            }

            IStepInspectorPanel panel = GetOrCreatePanel(panelId, step);
            if (panel == null)
            {
                return null;
            }

            IElementDrawerContext ctx = StepInspectorContext.For(step, changeCallback);
            return panel.BuildContent(step, ctx);
        }

        /// <summary>Override point for subclasses that want to expose extra panel ids.</summary>
        protected virtual IStepInspectorPanel CreatePanel(string panelId)
        {
            switch (panelId)
            {
                case PanelIds.Header:      return new StepHeaderPanel();
                case PanelIds.Behaviors:   return new BehaviorsTab();
                case PanelIds.Transitions: return new TransitionsTab();
                case PanelIds.Unlocked:    return new UnlockedObjectsTab();
                default: return null;
            }
        }

        private IStepInspectorPanel GetOrCreatePanel(string id, Step.EntityData step)
        {
            // Selecting a different step invalidates the whole cache so panels do not
            // hold references to a stale Step.EntityData.
            if (lastStep != step)
            {
                DisposeCachedPanels();
                lastStep = step;
            }

            if (panelCache.TryGetValue(id, out IStepInspectorPanel cached))
            {
                return cached;
            }

            IStepInspectorPanel panel = CreatePanel(id);
            if (panel != null)
            {
                panelCache[id] = panel;
            }
            return panel;
        }

        private void DisposeCachedPanels()
        {
            foreach (IStepInspectorPanel panel in panelCache.Values)
            {
                panel?.Dispose();
            }
            panelCache.Clear();
        }
    }
}
