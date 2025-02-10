using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.UI.Console;

namespace  VRBuilder.Core.Utils
{
    public static class VRConsole
    {
        private static ILogConsole console;
        
        static VRConsole()
        {
            
            Debug.Log("VR Console Initialized");
            console = RuntimeConfigurator.Configuration.VRConsole;

            if (console == null)
            {
                Debug.LogError("Could not initialize VR console.");
            }
            
            console.Hide();
        }

        public static void Log(string message)
        {
            LogMessage logMessage = new LogMessage(message);
            console.Log(logMessage);
            console.Show();
        }
    }
}

