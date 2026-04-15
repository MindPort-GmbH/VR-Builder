using UnityEngine.UIElements;

namespace VRBuilder.Netcode.UI.Keyboard
{
    public class UIToolkitTextFieldAdapter : ITextFieldAdapter
    {
        public TextField TextField { get; }

        public string Text
        {
            get => TextField.value;
            set => TextField.SetValueWithoutNotify(value ?? string.Empty);
        }

        public int CursorIndex
        {
            get => TextField.cursorIndex;
            set => TextField.cursorIndex = value;
        }

        public int SelectionIndex
        {
            get => TextField.selectIndex;
            set => TextField.selectIndex = value;
        }

        public int MaxLength => TextField.maxLength;

        public UIToolkitTextFieldAdapter(TextField textField)
        {
            TextField = textField;
        }

        public KeyboardTextState GetState()
        {
            return KeyboardTextState.FromRaw(Text, CursorIndex, SelectionIndex, MaxLength);
        }

        public void SetState(KeyboardTextState state, bool notify)
        {
            KeyboardTextState normalized = state.Normalized();
            if (notify)
            {
                TextField.value = normalized.Text;
            }
            else
            {
                TextField.SetValueWithoutNotify(normalized.Text);
            }

            CursorIndex = normalized.CursorIndex;
            SelectionIndex = normalized.SelectionIndex;
        }
    }
}
