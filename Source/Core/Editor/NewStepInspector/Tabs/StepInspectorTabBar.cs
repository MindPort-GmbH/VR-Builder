// Copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Tabs
{
    /// <summary>
    /// Manages a row of ToolbarToggle elements and swaps the visible content panel.
    /// Persists selected tab index via SessionState.
    /// </summary>
    public class StepInspectorTabBar
    {
        private const string SessionStateKey = "NewStepInspector_SelectedTab";

        private readonly List<IStepInspectorTab> tabs;
        private readonly List<ToolbarToggle> toggles = new List<ToolbarToggle>();
        private readonly List<VisualElement> contentPanels = new List<VisualElement>();
        private int selectedIndex;
        private bool suppressCallbacks;

        public StepInspectorTabBar(List<IStepInspectorTab> tabs)
        {
            this.tabs = tabs;
            selectedIndex = SessionState.GetInt(SessionStateKey, 0);
            if (selectedIndex >= tabs.Count)
            {
                selectedIndex = 0;
            }
        }

        /// <summary>
        /// Builds the toolbar with toggle buttons and content panels.
        /// Returns a container with the toolbar and all content panels.
        /// </summary>
        public VisualElement Build()
        {
            VisualElement root = new VisualElement();
            root.style.flexGrow = 1;

            Toolbar toolbar = new Toolbar();
            toolbar.AddToClassList("step-inspector__tabs-toolbar");

            for (int i = 0; i < tabs.Count; i++)
            {
                int closuredIndex = i;
                IStepInspectorTab tab = tabs[i];

                ToolbarToggle toggle = new ToolbarToggle { text = tab.TabLabel };
                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (suppressCallbacks) return;
                    if (evt.newValue)
                    {
                        SetActiveTab(closuredIndex);
                    }
                });

                toggles.Add(toggle);
                toolbar.Add(toggle);

                VisualElement contentPanel = tab.BuildContent();
                contentPanel.AddToClassList("step-inspector__panel");
                contentPanels.Add(contentPanel);
            }

            root.Add(toolbar);

            foreach (VisualElement panel in contentPanels)
            {
                root.Add(panel);
            }

            SetActiveTab(selectedIndex);
            return root;
        }

        /// <summary>
        /// Switches to the tab at the given index.
        /// </summary>
        public void SetActiveTab(int index)
        {
            if (index < 0 || index >= tabs.Count)
            {
                return;
            }

            selectedIndex = index;
            SessionState.SetInt(SessionStateKey, index);

            suppressCallbacks = true;
            for (int i = 0; i < toggles.Count; i++)
            {
                toggles[i].SetValueWithoutNotify(i == index);
                contentPanels[i].EnableInClassList("step-inspector__panel--hidden", i != index);
            }
            suppressCallbacks = false;
        }

        /// <summary>
        /// Refreshes all tabs with current step data.
        /// </summary>
        public void RefreshAll(IStep step, IChapter chapter, IProcess process)
        {
            foreach (IStepInspectorTab tab in tabs)
            {
                tab.Refresh(step, chapter, process);
            }

            UpdateWarningIcons();
        }

        /// <summary>
        /// Updates warning icons on tab toggles.
        /// </summary>
        public void UpdateWarningIcons()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                string label = tabs[i].TabLabel;
                if (tabs[i].HasWarning)
                {
                    label += " \u26A0";
                }
                toggles[i].text = label;
            }
        }

        /// <summary>
        /// Disposes all tabs.
        /// </summary>
        public void Dispose()
        {
            foreach (IStepInspectorTab tab in tabs)
            {
                tab.Dispose();
            }
        }
    }
}
