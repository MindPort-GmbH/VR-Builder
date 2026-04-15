namespace VRBuilder.Netcode.UI.Keyboard
{
    public interface ITextFieldAdapter
    {
        string Text { get; set; }
        int CursorIndex { get; set; }
        int SelectionIndex { get; set; }
        int MaxLength { get; }
        KeyboardTextState GetState();
        void SetState(KeyboardTextState state, bool notify);
    }
}
