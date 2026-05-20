namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.DragDrop
{
    /// <summary>
    /// String tags used to gate drag/drop compatibility — a "behavior" drag cannot land on
    /// a "condition" drop target. Keep these as string constants so third-party tabs can
    /// invent their own kinds without enum changes here.
    /// </summary>
    public static class DragKinds
    {
        public const string Behavior = "behavior";
        public const string Condition = "condition";
        public const string Transition = "transition";
    }
}
