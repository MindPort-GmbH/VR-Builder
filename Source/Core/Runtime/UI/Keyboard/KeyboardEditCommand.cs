namespace VRBuilder.Core.UI.Keyboard
{
    /// <summary>
    /// Immutable value object describing a single edit the keyboard wants to perform on a text field:
    /// a <see cref="KeyboardEditAction"/> plus optional text payload (used by <see cref="KeyboardEditAction.InsertText"/>).
    /// Consumed by <see cref="KeyboardTextEditing.Apply"/> to produce the next <see cref="KeyboardTextState"/>.
    /// </summary>
    public readonly struct KeyboardEditCommand
    {
        /// <summary>The operation this command represents.</summary>
        public KeyboardEditAction Action { get; }

        /// <summary>
        /// Text payload for the command. Only meaningful when <see cref="Action"/> is
        /// <see cref="KeyboardEditAction.InsertText"/>; empty for all other actions.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Creates a new command. <paramref name="text"/> is normalized to an empty string when null.
        /// </summary>
        public KeyboardEditCommand(KeyboardEditAction action, string text = null)
        {
            Action = action;
            Text = text ?? string.Empty;
        }

        /// <summary>Shorthand for a no-op command.</summary>
        public static KeyboardEditCommand None => new KeyboardEditCommand(KeyboardEditAction.None);

        /// <summary>Shorthand for a backspace command.</summary>
        public static KeyboardEditCommand Backspace => new KeyboardEditCommand(KeyboardEditAction.Backspace);

        /// <summary>Shorthand for a forward-delete command.</summary>
        public static KeyboardEditCommand Delete => new KeyboardEditCommand(KeyboardEditAction.Delete);

        /// <summary>Shorthand for a clear-all command.</summary>
        public static KeyboardEditCommand Clear => new KeyboardEditCommand(KeyboardEditAction.Clear);

        /// <summary>Shorthand for a submit command.</summary>
        public static KeyboardEditCommand Submit => new KeyboardEditCommand(KeyboardEditAction.Submit);

        /// <summary>Shorthand for a close command.</summary>
        public static KeyboardEditCommand Close => new KeyboardEditCommand(KeyboardEditAction.Close);

        /// <summary>
        /// Creates an <see cref="KeyboardEditAction.InsertText"/> command that will insert <paramref name="value"/>
        /// at the cursor (replacing any active selection).
        /// </summary>
        public static KeyboardEditCommand Insert(string value)
        {
            return new KeyboardEditCommand(KeyboardEditAction.InsertText, value);
        }
    }
}
