using System;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.UI.Console;

namespace VRBuilder.Core.Utils
{
    public static class VRConsole
    {
        private static ILogConsole console;

        static VRConsole()
        {
            console = RuntimeConfigurator.Configuration.VRConsole;

            if (console == null)
            {
                Debug.LogError("Could not initialize VR console.");
            }

            console.Hide();
        }

        public static void Log(string message, string details = "")
        {
            console.LogMessage(message, details, LogType.Log);
            console.Show();
        }

        public static void LogWarning(string message, string details = "")
        {
            console.LogMessage(message, details, LogType.Warning);
            console.Show();
        }

        public static void LogError(string message, string details = "")
        {
            console.LogMessage(message, details, LogType.Error);
            console.Show();
        }

        public static void LogException(Exception ex)
        {
            console.LogMessage(ex.Message, ex.StackTrace, LogType.Exception);
            console.Show();
        }
    }
}

