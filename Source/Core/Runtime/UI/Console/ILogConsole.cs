using UnityEngine;

namespace  VRBuilder.UI.Console
{
    public interface ILogConsole
    {
        void Log(string message, string details = "");
        
        void LogWarning(string message, string details = "");
        
        void LogError(string message, string details = "");
        
        void LogException(System.Exception ex);

        void Clear();

        void Show();
        
        void Hide();
    }
}
