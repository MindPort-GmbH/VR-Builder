using System;

namespace VRBuilder.Core.UI.Keyboard
{
    /// <summary>
    /// Pure text-editing logic — takes a <see cref="KeyboardTextState"/> and a
    /// <see cref="KeyboardEditCommand"/> and returns the resulting state. No Unity dependencies,
    /// no side effects; backends call this to keep their in-memory mirror consistent before pushing
    /// the result out through <see cref="IKeyboardBackend.StateUpdated"/>.
    /// </summary>
    public static class KeyboardTextEditing
    {
        /// <summary>
        /// Applies <paramref name="command"/> to <paramref name="state"/> and returns the new state.
        /// Handles selection replacement, max-length clipping for inserts, and no-ops
        /// (<see cref="KeyboardEditAction.Submit"/>/<see cref="KeyboardEditAction.Close"/>/<see cref="KeyboardEditAction.None"/>)
        /// which simply return the normalized input state.
        /// </summary>
        public static KeyboardTextState Apply(KeyboardTextState state, KeyboardEditCommand command)
        {
            KeyboardTextState normalizedState = state.Normalized();
            string text = normalizedState.Text;
            int cursor = normalizedState.CursorIndex;
            int selection = normalizedState.SelectionIndex;
            int maxLength = normalizedState.MaxLength;

            int start = Math.Min(cursor, selection);
            int end = Math.Max(cursor, selection);
            bool hasSelection = start != end;

            switch (command.Action)
            {
                case KeyboardEditAction.InsertText:
                    if (string.IsNullOrEmpty(command.Text))
                    {
                        return normalizedState;
                    }

                    string insertText = command.Text;
                    if (maxLength > 0)
                    {
                        int fixedCount = text.Length - (end - start);
                        int remaining = maxLength - fixedCount;
                        if (remaining <= 0)
                        {
                            return normalizedState;
                        }

                        if (insertText.Length > remaining)
                        {
                            insertText = insertText.Substring(0, remaining);
                        }
                    }

                    string textBeforeInsert = hasSelection ? RemoveRange(text, start, end) : text;
                    int insertAt = hasSelection ? start : cursor;
                    string inserted = textBeforeInsert.Insert(insertAt, insertText);
                    int newCaret = insertAt + insertText.Length;
                    return KeyboardTextState.FromRaw(inserted, newCaret, newCaret, maxLength);

                case KeyboardEditAction.Backspace:
                    if (hasSelection)
                    {
                        string removedSelection = RemoveRange(text, start, end);
                        return KeyboardTextState.FromRaw(removedSelection, start, start, maxLength);
                    }

                    if (cursor <= 0)
                    {
                        return normalizedState;
                    }

                    string removedBackspace = text.Remove(cursor - 1, 1);
                    int backspaceCaret = cursor - 1;
                    return KeyboardTextState.FromRaw(removedBackspace, backspaceCaret, backspaceCaret, maxLength);

                case KeyboardEditAction.Delete:
                    if (hasSelection)
                    {
                        string removedSelectionDelete = RemoveRange(text, start, end);
                        return KeyboardTextState.FromRaw(removedSelectionDelete, start, start, maxLength);
                    }

                    if (cursor >= text.Length)
                    {
                        return normalizedState;
                    }

                    string removedDelete = text.Remove(cursor, 1);
                    return KeyboardTextState.FromRaw(removedDelete, cursor, cursor, maxLength);

                case KeyboardEditAction.Clear:
                    return KeyboardTextState.FromRaw(string.Empty, 0, 0, maxLength);

                case KeyboardEditAction.Submit:
                case KeyboardEditAction.Close:
                case KeyboardEditAction.None:
                default:
                    return normalizedState;
            }
        }

        private static string RemoveRange(string source, int start, int end)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }

            int clampedStart = Math.Clamp(start, 0, source.Length);
            int clampedEnd = Math.Clamp(end, 0, source.Length);
            if (clampedStart >= clampedEnd)
            {
                return source;
            }

            return source.Remove(clampedStart, clampedEnd - clampedStart);
        }
    }
}
