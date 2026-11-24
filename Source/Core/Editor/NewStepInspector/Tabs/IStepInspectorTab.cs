// Copyright (c) 2021-2025 MindPort GmbH

using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Tabs
{
    /// <summary>
    /// Interface for a tab in the new UIToolkit Step Inspector.
    /// Each tab builds its own content panel and can be refreshed when the step changes.
    /// </summary>
    public interface IStepInspectorTab
    {
        /// <summary>
        /// Display label for the tab toggle.
        /// </summary>
        string TabLabel { get; }

        /// <summary>
        /// Whether this tab has validation warnings.
        /// </summary>
        bool HasWarning { get; }

        /// <summary>
        /// Builds the tab content panel. Called once when the tab is first created.
        /// </summary>
        VisualElement BuildContent();

        /// <summary>
        /// Refreshes the tab with current step data.
        /// Called when the step selection changes or data is modified externally.
        /// </summary>
        void Refresh(IStep step, IChapter chapter, IProcess process);

        /// <summary>
        /// Called when the tab is being destroyed or the window is closing.
        /// </summary>
        void Dispose();
    }
}
