namespace VRBuilder.Netcode.UI.Keyboard
{
    public enum KeyboardEditAction
    {
        None = 0,
        InsertText = 1,
        Backspace = 2,
        Delete = 3,
        Clear = 4,
        Submit = 5,
        Close = 6,
    }

    public readonly struct KeyboardEditCommand
    {
        public KeyboardEditAction Action { get; }
        public string Text { get; }

        public KeyboardEditCommand(KeyboardEditAction action, string text = null)
        {
            Action = action;
            Text = text ?? string.Empty;
        }

        public static KeyboardEditCommand None => new KeyboardEditCommand(KeyboardEditAction.None);
        public static KeyboardEditCommand Backspace => new KeyboardEditCommand(KeyboardEditAction.Backspace);
        public static KeyboardEditCommand Delete => new KeyboardEditCommand(KeyboardEditAction.Delete);
        public static KeyboardEditCommand Clear => new KeyboardEditCommand(KeyboardEditAction.Clear);
        public static KeyboardEditCommand Submit => new KeyboardEditCommand(KeyboardEditAction.Submit);
        public static KeyboardEditCommand Close => new KeyboardEditCommand(KeyboardEditAction.Close);

        public static KeyboardEditCommand Insert(string value)
        {
            return new KeyboardEditCommand(KeyboardEditAction.InsertText, value);
        }
    }
}
