using System;
using System.Collections.Generic;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.UI.Console;

namespace VRBuilder.Core.Utils
{
    public static class VRConsole
    {
        private static ILogConsole console;
        private static Queue<Action> executionQueue = new Queue<Action>();

        static VRConsole()
        {
            console = RuntimeConfigurator.Configuration.VRConsole;

            if (console == null)
            {
                Debug.LogError("Could not initialize VR console.");
            }

            console.Hide();
        }

        public static void Refresh()
        {
            if (console is null)
            {
                return;
            }

            lock (executionQueue)
            {
                while (executionQueue.Count > 0)
                {
                    executionQueue.Dequeue()?.Invoke();
                }
            }
        }

        public static void Log(string message, string details = "")
        {
            lock (executionQueue)
            {
                executionQueue.Enqueue(() =>
                {
                    console.LogMessage(message, details, LogType.Log);
                    console.Show();
                });
            }
        }

        public static void LogWarning(string message, string details = "")
        {
            lock (executionQueue)
            {
                executionQueue.Enqueue(() =>
                {
                    console.LogMessage(message, details, LogType.Warning);
                    console.Show();
                });
            }
        }

        public static void LogError(string message, string details = "")
        {
            lock (executionQueue)
            {
                executionQueue.Enqueue(() =>
                {
                    console.LogMessage(message, details, LogType.Error);
                    console.Show();
                });
            }
        }

        public static void LogException(Exception ex)
        {
            lock (executionQueue)
            {
                executionQueue.Enqueue(() =>
                {
                    console.LogMessage(ex.Message, ex.StackTrace, LogType.Exception);
                    console.Show();
                });
            }
        }
    }
}

