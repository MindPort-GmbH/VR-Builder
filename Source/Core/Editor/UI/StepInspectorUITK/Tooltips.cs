namespace VRBuilder.Core.Editor.UI.StepInspectorUITK
{
    /// <summary>
    /// Centralized tooltip strings. Edit a value here and every button across the inspector
    /// updates — useful for translation passes or for keeping wording consistent.
    /// </summary>
    public static class Tooltips
    {
        // Generic
        public const string Grip = "Drag to reorder or move to another list";

        // Add buttons
        public const string AddBehavior = "Add a new behavior to this step";
        public const string AddCondition = "Add a new condition to this transition";
        public const string AddTransition = "Add a new transition leaving this step";

        // Header actions
        public const string DeleteBehavior = "Remove this behavior";
        public const string DeleteCondition = "Remove this condition";
        public const string DeleteTransition = "Remove this transition";
        public const string EntityMenu = "Menu — copy, paste, remove";
        public const string EntityHelp = "Open documentation for this entity";

        // Scene reference
        public const string SceneRefShow = "Show — ping the referenced object in the scene";
        public const string SceneRefEdit = "Edit groups — add a Scene Object Group to this reference";
        public const string SceneRefClear = "Clear the reference";
        public const string SceneRefDropZone = "Drop a Process Scene Object (or any GameObject) here to assign it.";

        // Unlocked objects tab
        public const string UnlockedRemoveObject = "Remove this object from the manual unlock list";
        public const string UnlockedAutoLocked = "Locked on — this property is automatically unlocked by a condition";
        public const string UnlockedToggleManual = "Toggle manual unlock for this property";
        public const string UnlockedRemoveGroup = "Remove this group from the unlock list";
        public const string UnlockedAddGroup = "Pick a Scene Object Group whose properties should be unlocked for this step";
        public const string UnlockedDragHint = "Drag a Process Scene Object here to add it:";

        // Background behavior toggle (decoration)
        public const string WaitForCompletion =
            "When checked, the step waits for this behavior to finish before moving on.";

        // Audio preview
        public const string AudioPreview = "Play the configured audio clip in the editor";
    }
}
