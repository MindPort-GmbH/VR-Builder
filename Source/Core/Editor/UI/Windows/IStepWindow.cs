namespace VRBuilder.Core.Editor.UI.Windows
{
    /// <summary>
    /// Marker for editor windows that host the Step Inspector: the legacy IMGUI
    /// <see cref="StepWindow"/> and the UITK <see cref="StepInspectorUITK.Windows.DetachedPanelWindow"/>.
    /// Lets window management code (closing, open detection) treat every step inspector
    /// uniformly instead of hard-coding the concrete <see cref="StepWindow"/> type, so both
    /// inspectors behave the same way. Implementers are always <see cref="UnityEditor.EditorWindow"/>s.
    /// </summary>
    public interface IStepWindow
    {
    }
}
