using UnityEditor;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    // Typed wrappers around DetachedPanelWindow, one per Step Inspector panel. Unity's native
    // "Add Tab" menu lists EditorWindow types (not instances) and creates them arg-less, so each
    // panel needs its own concrete type. [EditorWindowTitle] sets the Add Tab menu label; a "/"
    // creates a submenu (e.g. Add Tab → VR Builder → Step). Docked tab titles stay short via
    // DetachedPanelWindow.TitleFor. DefaultPanelId tells the shared base which panel to render.

    [EditorWindowTitle(title = "VR Builder/Step")]
    public sealed class StepTabWindow : DetachedPanelWindow
    {
        protected override string DefaultPanelId => PanelIds.Header;
    }

    [EditorWindowTitle(title = "VR Builder/Behaviors")]
    public sealed class BehaviorsTabWindow : DetachedPanelWindow
    {
        protected override string DefaultPanelId => PanelIds.Behaviors;
    }

    [EditorWindowTitle(title = "VR Builder/Transitions")]
    public sealed class TransitionsTabWindow : DetachedPanelWindow
    {
        protected override string DefaultPanelId => PanelIds.Transitions;
    }

    [EditorWindowTitle(title = "VR Builder/Unlocked Objects")]
    public sealed class UnlockedObjectsTabWindow : DetachedPanelWindow
    {
        protected override string DefaultPanelId => PanelIds.Unlocked;
    }
}
