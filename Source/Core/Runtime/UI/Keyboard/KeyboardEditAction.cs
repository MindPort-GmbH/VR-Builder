namespace VRBuilder.Core.UI.Keyboard
{
    /// <summary>
    /// High-level edit operations that a keyboard backend can request against a <see cref="KeyboardTextState"/>.
    /// Used as the <c>Action</c> component of a <see cref="KeyboardEditCommand"/>.
    /// </summary>
    public enum KeyboardEditAction
    {
        /// <summary>No-op; the state is returned unchanged.</summary>
        None = 0,

        /// <summary>Insert the command's text at the current cursor position (replacing any selection).</summary>
        InsertText = 1,

        /// <summary>Remove the character before the cursor, or the current selection if one exists.</summary>
        Backspace = 2,

        /// <summary>Remove the character after the cursor, or the current selection if one exists.</summary>
        Delete = 3,

        /// <summary>Clear all text and reset the cursor to the start.</summary>
        Clear = 4,

        /// <summary>Commit the current text (the backend typically closes afterwards).</summary>
        Submit = 5,

        /// <summary>Close the keyboard without committing.</summary>
        Close = 6,
    }
}
