using UnityEngine;

namespace VRBuilder.UI.Console
{
    public struct LogMessage
    {
        public string Message { get; private set; }
        public string Details { get; private set; }
        public LogType LogType { get; private set; }

        public LogMessage(string message, string details, LogType logType)
        {
            Message = message;
            Details = details;
            LogType = logType;
        }
    }
}
