using System;
using UnityEngine;

namespace VRBuilder.Core.UI.Keyboard
{
    /// <summary>
    /// Immutable snapshot of a text field's editing state: the text itself, caret position,
    /// selection anchor, and max length. Used as the common currency between text-field adapters,
    /// the editing logic in <see cref="KeyboardTextEditing"/>, and keyboard backends.
    /// Marked <see cref="SerializableAttribute"/> so it can be inspected and net-serialized.
    /// </summary>
    [Serializable]
    public struct KeyboardTextState : IEquatable<KeyboardTextState>
    {
        [SerializeField]
        private string text;

        [SerializeField]
        private int cursorIndex;

        [SerializeField]
        private int selectionIndex;

        [SerializeField]
        private int maxLength;

        /// <summary>The text content. Never null (empty string if unset).</summary>
        public string Text => text ?? string.Empty;

        /// <summary>Zero-based caret position.</summary>
        public int CursorIndex => cursorIndex;

        /// <summary>Selection anchor index. Equal to <see cref="CursorIndex"/> when nothing is selected.</summary>
        public int SelectionIndex => selectionIndex;

        /// <summary>Max text length. Non-positive means unlimited.</summary>
        public int MaxLength => maxLength;

        /// <summary>True when <see cref="CursorIndex"/> and <see cref="SelectionIndex"/> differ, i.e. a range is selected.</summary>
        public bool HasSelection => cursorIndex != selectionIndex;

        /// <summary>
        /// Creates a new state. Prefer <see cref="FromRaw"/> when the inputs may need clamping —
        /// this constructor stores the values as given.
        /// </summary>
        public KeyboardTextState(string value, int cursor, int selection, int maximumLength = -1)
        {
            text = value ?? string.Empty;
            cursorIndex = cursor;
            selectionIndex = selection;
            maxLength = maximumLength;
        }

        /// <summary>
        /// Creates a normalized state from potentially unchecked values. Clamps cursor/selection to
        /// the text length and treats <c>0</c> as "unlimited" for <paramref name="maximumLength"/>.
        /// </summary>
        public static KeyboardTextState FromRaw(string value, int cursor, int selection, int maximumLength = -1)
        {
            return new KeyboardTextState(value, cursor, selection, maximumLength).Normalized();
        }

        /// <summary>Returns a new state with different text/cursor/selection but the same <see cref="MaxLength"/>.</summary>
        public KeyboardTextState With(string value, int cursor, int selection)
        {
            return FromRaw(value, cursor, selection, maxLength);
        }

        /// <summary>
        /// Returns a cleaned-up copy: null text becomes empty, cursor/selection are clamped into
        /// the valid range, and a zero max length is treated as "unlimited" (<c>-1</c>).
        /// </summary>
        public KeyboardTextState Normalized()
        {
            string normalizedText = text ?? string.Empty;
            int clampedCursor = Mathf.Clamp(cursorIndex, 0, normalizedText.Length);
            int clampedSelection = Mathf.Clamp(selectionIndex, 0, normalizedText.Length);
            int normalizedMaxLength = maxLength == 0 ? -1 : maxLength;
            return new KeyboardTextState(normalizedText, clampedCursor, clampedSelection, normalizedMaxLength);
        }

        /// <summary>Value-equality against another state (all four fields must match).</summary>
        public bool Equals(KeyboardTextState other)
        {
            return Text == other.Text &&
                   CursorIndex == other.CursorIndex &&
                   SelectionIndex == other.SelectionIndex &&
                   MaxLength == other.MaxLength;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is KeyboardTextState other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Text, CursorIndex, SelectionIndex, MaxLength);
        }
    }
}
