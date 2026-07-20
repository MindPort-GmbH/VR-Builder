using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs
{
    /// <summary>
    /// One section of the Step Inspector (header, behaviors, transitions, unlocked objects).
    /// Owned by <see cref="Drawers.StepElementDrawer"/>; consumed by windows that host
    /// the section's <see cref="VisualElement"/>.
    /// </summary>
    public interface IStepInspectorPanel : IDisposable
    {
        /// <summary>Stable panel id, one of <see cref="PanelIds"/>.</summary>
        string Id { get; }

        /// <summary>Title shown on the panel header and its dock tab.</summary>
        GUIContent Label { get; }

        /// <summary>Builds the panel's UI for <paramref name="step"/>. Called fresh on every rebuild.</summary>
        VisualElement BuildContent(IStepData step);

        // Dispose (from IDisposable) is the cleanup hook: StepElementDrawer caches one panel
        // instance per id and calls Dispose when the selected step changes. Panels that
        // subscribe to events or hold disposable resources release them here.
    }
}
