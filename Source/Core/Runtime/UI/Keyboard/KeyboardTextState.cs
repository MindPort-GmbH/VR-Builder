using System;
using UnityEngine;

namespace VRBuilder.Netcode.UI.Keyboard
{
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

        public string Text => text ?? string.Empty;
        public int CursorIndex => cursorIndex;
        public int SelectionIndex => selectionIndex;
        public int MaxLength => maxLength;
        public bool HasSelection => cursorIndex != selectionIndex;

        public KeyboardTextState(string value, int cursor, int selection, int maximumLength = -1)
        {
            text = value ?? string.Empty;
            cursorIndex = cursor;
            selectionIndex = selection;
            maxLength = maximumLength;
        }

        public static KeyboardTextState FromRaw(string value, int cursor, int selection, int maximumLength = -1)
        {
            return new KeyboardTextState(value, cursor, selection, maximumLength).Normalized();
        }

        public KeyboardTextState With(string value, int cursor, int selection)
        {
            return FromRaw(value, cursor, selection, maxLength);
        }

        public KeyboardTextState Normalized()
        {
            string normalizedText = text ?? string.Empty;
            int clampedCursor = Mathf.Clamp(cursorIndex, 0, normalizedText.Length);
            int clampedSelection = Mathf.Clamp(selectionIndex, 0, normalizedText.Length);
            int normalizedMaxLength = maxLength == 0 ? -1 : maxLength;
            return new KeyboardTextState(normalizedText, clampedCursor, clampedSelection, normalizedMaxLength);
        }

        public bool Equals(KeyboardTextState other)
        {
            return Text == other.Text &&
                   CursorIndex == other.CursorIndex &&
                   SelectionIndex == other.SelectionIndex &&
                   MaxLength == other.MaxLength;
        }

        public override bool Equals(object obj)
        {
            return obj is KeyboardTextState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Text, CursorIndex, SelectionIndex, MaxLength);
        }
    }
}
