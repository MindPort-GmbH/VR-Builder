using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers
{
    /// <summary>
    /// Per-window panel cache and builder. Extracted from <see cref="StepElementDrawer"/>
    /// so multiple <see cref="Windows.DetachedPanelWindow"/> instances can each render a
    /// different step (e.g. when one panel is locked and another follows graph selection).
    /// </summary>
    internal sealed class StepPanelBuildContext : IDisposable
    {
        private readonly Func<string, IStepInspectorPanel> createPanel;
        private readonly Dictionary<string, IStepInspectorPanel> panelCache = new Dictionary<string, IStepInspectorPanel>();
        private Step.EntityData lastStep;

        public StepPanelBuildContext(Func<string, IStepInspectorPanel> createPanel)
        {
            this.createPanel = createPanel ?? throw new ArgumentNullException(nameof(createPanel));
        }

        public VisualElement BuildPanel(string panelId, Step.EntityData step)
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

            return panel.BuildContent(step);
        }

        public void Dispose()
        {
            DisposeCachedPanels();
            lastStep = null;
        }

        private IStepInspectorPanel GetOrCreatePanel(string id, Step.EntityData step)
        {
            if (lastStep != step)
            {
                DisposeCachedPanels();
                lastStep = step;
            }

            if (panelCache.TryGetValue(id, out IStepInspectorPanel cached))
            {
                return cached;
            }

            IStepInspectorPanel panel = createPanel(id);
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
