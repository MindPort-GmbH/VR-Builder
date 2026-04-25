namespace VRBuilder.Netcode.UI.Keyboard
{
    /// <summary>
    /// Abstraction over a text input field so the keyboard layer doesn't have to know whether
    /// the underlying widget is a UIToolkit <c>TextField</c>, a UGUI <c>InputField</c>, or something else.
    /// Implementations expose read/write access to the text, caret and selection, plus a normalized
    /// state snapshot.
    /// </summary>
    public interface ITextFieldAdapter
    {
        /// <summary>The current text of the underlying field.</summary>
        string Text { get; set; }

        /// <summary>Zero-based caret position within <see cref="Text"/>.</summary>
        int CursorIndex { get; set; }

        /// <summary>
        /// Anchor index of the selection. When equal to <see cref="CursorIndex"/> there is no selection.
        /// </summary>
        int SelectionIndex { get; set; }

        /// <summary>Maximum allowed length. Non-positive values mean "unlimited".</summary>
        int MaxLength { get; }

        /// <summary>Returns a normalized <see cref="KeyboardTextState"/> snapshot of the current field.</summary>
        KeyboardTextState GetState();

        /// <summary>
        /// Applies the given state to the field.
        /// When <paramref name="notify"/> is true the underlying widget fires its change callbacks,
        /// otherwise the change is applied silently (used when mirroring the backend to avoid feedback loops).
        /// </summary>
        void SetState(KeyboardTextState state, bool notify);
    }
}
