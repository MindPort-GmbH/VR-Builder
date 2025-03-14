using UnityEngine;

namespace VRBuilder.UI.Console
{
    /// <summary>
    /// A console for logging debug messages.
    /// </summary>
    public interface ILogConsole
    {
        /// <summary>
        /// Add the provided message to the log.
        /// </summary>
        /// <param name="message">Main message.</param>
        /// <param name="details">Extra details, e.g. stack trace.</param>
        /// <param name="logType">Type of message logged.</param>
        void LogMessage(string message, string details, LogType logType);

        /// <summary>
        /// Clears the console of all messages.
        /// </summary>
        void Clear();

        /// <summary>
        /// Makes the console visible.
        /// </summary>
        void Show();

        /// <summary>
        /// Hides the console.
        /// </summary>
        void Hide();

        /// <summary>
        /// Manually sets the console dirty, so it knows it has to be refreshed.
        /// </summary>
        void SetDirty();
    }
}
