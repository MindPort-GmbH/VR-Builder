using UnityEditor;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    // Typed wrappers around DetachedPanelWindow, one per Step Inspector panel. Unity's native
    // "Add Tab" menu lists EditorWindow types (not instances) and creates them arg-less, so each
    // panel needs its own concrete type. [EditorWindowTitle] sets the exact menu/tab label; the
    // DefaultPanelId override tells the shared base which panel to render. See DetachedPanelWindow.

    [EditorWindowTitle(title = "Step")]
    public sealed class StepTabWindow : DetachedPanelWindow
    {
        protected override string DefaultPanelId => PanelIds.Header;
    }

    [EditorWindowTitle(title = "Behaviors")]
    public sealed class BehaviorsTabWindow : DetachedPanelWindow
    {
        protected override string DefaultPanelId => PanelIds.Behaviors;
    }

    [EditorWindowTitle(title = "Transitions")]
    public sealed class TransitionsTabWindow : DetachedPanelWindow
    {
        protected override string DefaultPanelId => PanelIds.Transitions;
    }

    [EditorWindowTitle(title = "Unlocked Objects")]
    public sealed class UnlockedObjectsTabWindow : DetachedPanelWindow
    {
        protected override string DefaultPanelId => PanelIds.Unlocked;
    }
}
