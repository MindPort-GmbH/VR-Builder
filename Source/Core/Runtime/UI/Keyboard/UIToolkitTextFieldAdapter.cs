using UnityEngine.UIElements;

namespace VRBuilder.Core.UI.Keyboard
{
    /// <summary>
    /// <see cref="ITextFieldAdapter"/> implementation that wraps a Unity UIToolkit <see cref="TextField"/>.
    /// Translates the adapter's generic read/write operations into UIToolkit-specific calls and builds
    /// <see cref="KeyboardTextState"/> snapshots so the keyboard layer never touches UIToolkit directly.
    /// </summary>
    public class UIToolkitTextFieldAdapter : ITextFieldAdapter
    {
        /// <summary>The underlying UIToolkit TextField this adapter is bound to.</summary>
        public TextField TextField { get; }

        /// <summary>
        /// Current text of the field. The setter uses <c>SetValueWithoutNotify</c> so assigning here
        /// does NOT trigger UIToolkit's ChangeEvent (use <see cref="SetState"/> with <c>notify: true</c>
        /// if you want listeners to fire).
        /// </summary>
        public string Text
        {
            get => TextField.value;
            set => TextField.SetValueWithoutNotify(value ?? string.Empty);
        }

        /// <summary>Caret position within the field.</summary>
        public int CursorIndex
        {
            get => TextField.cursorIndex;
            set => TextField.cursorIndex = value;
        }

        /// <summary>Selection anchor within the field. Equal to <see cref="CursorIndex"/> when nothing is selected.</summary>
        public int SelectionIndex
        {
            get => TextField.selectIndex;
            set => TextField.selectIndex = value;
        }

        /// <summary>Max length configured on the underlying TextField. Non-positive means unlimited.</summary>
        public int MaxLength => TextField.maxLength;

        /// <summary>Creates a new adapter bound to <paramref name="textField"/>.</summary>
        public UIToolkitTextFieldAdapter(TextField textField)
        {
            TextField = textField;
        }

        /// <summary>Returns a normalized snapshot of the field's current text/cursor/selection/max-length.</summary>
        public KeyboardTextState GetState()
        {
            return KeyboardTextState.FromRaw(Text, CursorIndex, SelectionIndex, MaxLength);
        }

        /// <summary>
        /// Pushes <paramref name="state"/> back into the TextField.
        /// When <paramref name="notify"/> is true the assignment goes through <c>TextField.value</c>
        /// (fires ChangeEvent); otherwise it uses <c>SetValueWithoutNotify</c>. Silent updates are used
        /// when mirroring backend state back into the field to avoid event feedback loops.
        /// </summary>
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
