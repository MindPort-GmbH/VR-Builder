using UnityEngine;

namespace VRBuilder.UI.Console
{
    public interface ILogConsole
    {
        void LogMessage(string message, string details, LogType logType);

        void Clear();

        void Show();

        void Hide();
    }
}
