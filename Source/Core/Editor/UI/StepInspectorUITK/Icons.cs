namespace VRBuilder.Core.Editor.UI.StepInspectorUITK
{
    /// <summary>
    /// Centralized glyphs used on the inspector's icon buttons. Change a value here and
    /// every button across the inspector updates — useful when swapping to image-based
    /// icons or to a different glyph set later.
    /// </summary>
    public static class Icons
    {
        // Header / row buttons
        public const string Delete = "✕";
        public const string Menu = "⋮";
        public const string Help = "?";
        public const string Caret = "▼";
        public const string CaretCollapsed = "▶";

        // Scene-reference icon row
        public const string Info = "ⓘ";
        public const string Edit = "✎";

        // Drag handle PNG — see GripElement and Resources/icon_grip_*.
    }
}
