using System;

namespace VRBuilder.Core.UI.Keyboard
{
    /// <summary>
    /// Contract for a concrete keyboard implementation (e.g. an XR spatial keyboard).
    /// The backend is what actually shows the keyboard UI and emits key events; the bridge layer
    /// (<see cref="UITKKeyboardBridge"/>) translates those events into updates on a text field.
    /// </summary>
    public interface IKeyboardBackend
    {
        /// <summary>True when the backend can be used right now (its underlying keyboard asset exists).</summary>
        bool IsAvailable { get; }

        /// <summary>True when the keyboard is currently open and accepting input.</summary>
        bool IsOpen { get; }

        /// <summary>Fired whenever the backend's internal text state changes (e.g. a key was pressed).</summary>
        event Action<KeyboardTextState> StateUpdated;

        /// <summary>Fired when the user commits the text (e.g. pressing Enter/Submit). Passes the final text.</summary>
        event Action<string> Submitted;

        /// <summary>Fired when the keyboard closes for any reason (user close, programmatic close, etc.).</summary>
        event Action Closed;

        /// <summary>
        /// Open the keyboard pre-populated with the given state (text + caret + selection).
        /// </summary>
        void Open(KeyboardTextState state);

        /// <summary>
        /// Push an externally-edited state into the keyboard so it stays in sync when the user types
        /// on hardware or otherwise changes the field outside of the keyboard.
        /// </summary>
        void SyncState(KeyboardTextState state);

        /// <summary>Close the keyboard. Safe to call when already closed.</summary>
        void Close();
    }
}
