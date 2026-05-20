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
        string Id { get; }
        GUIContent Label { get; }

        VisualElement BuildContent(IStepData step, IElementDrawerContext ctx);
        void Refresh();
    }
}
