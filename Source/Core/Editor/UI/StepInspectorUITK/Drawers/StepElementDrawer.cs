using System;
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
    public class StepElementDrawer : ObjectElementDrawer, IStepPanelDrawer
    {
        private readonly StepPanelBuildContext panelBuildContext;

        public StepElementDrawer()
        {
            panelBuildContext = new StepPanelBuildContext(CreatePanel);
        }

        /// <summary>
        /// Creates an isolated build context for a detached panel window. Each window owns
        /// its own context so locked panels can retain a different step than the selection.
        /// </summary>
        internal StepPanelBuildContext CreatePanelBuildContext()
        {
            return new StepPanelBuildContext(CreatePanel);
        }

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
                VisualElement panelContent = BuildPanel(panelId, step);
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
        public VisualElement BuildPanel(string panelId, Step.EntityData step)
        {
            return panelBuildContext.BuildPanel(panelId, step);
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
    }
}
