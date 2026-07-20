namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs
{
    /// <summary>
    /// Stable identifiers for the panels a <see cref="Drawers.StepElementDrawer"/> exposes.
    /// Windows reference panels by id so a custom step drawer can rearrange them without
    /// breaking the shell.
    /// </summary>
    public static class PanelIds
    {
        public const string Header = "header";
        public const string Behaviors = "behaviors";
        public const string Transitions = "transitions";
        public const string Unlocked = "unlocked";

        public static readonly string[] AllInOrder =
        {
            Header,
            Behaviors,
            Transitions,
            Unlocked,
        };
    }
}
