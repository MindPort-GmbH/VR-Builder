using UnityEngine;

namespace  VRBuilder.UI.Console
{
    public interface ILogConsole
    {
        void Log(ILogMessage logMessage);

        void Clear();

        void Show();
        
        void Hide();
    }
}
