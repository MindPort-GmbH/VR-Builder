using System;

namespace VRBuilder.Netcode.UI.Keyboard
{
    public interface IKeyboardBackend
    {
        bool IsAvailable { get; }
        bool IsOpen { get; }

        event Action<KeyboardTextState> StateUpdated;
        event Action<string> Submitted;
        event Action Closed;

        void Open(KeyboardTextState state);
        void SyncState(KeyboardTextState state);
        void Close();
    }
}
