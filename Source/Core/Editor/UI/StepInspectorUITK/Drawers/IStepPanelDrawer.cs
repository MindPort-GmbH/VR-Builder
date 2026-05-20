using System;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers
{
    /// <summary>
    /// Optional capability for drawers that render a step as a set of named panels
    /// (header, behaviors, transitions, ...). <see cref="Windows.DetachedPanelWindow"/>
    /// uses this to host one panel per window without depending on a concrete drawer type.
    /// </summary>
    public interface IStepPanelDrawer
    {
        VisualElement BuildPanel(string panelId, Step.EntityData step, Action<object> changeCallback);
    }
}
