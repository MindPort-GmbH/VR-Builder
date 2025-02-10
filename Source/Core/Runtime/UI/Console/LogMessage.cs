using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.UI.Console
{
    public class LogMessage: ILogMessage
    {
        private string message;
        
        public void Bind(VisualElement messageElement)
        {
            Label label = messageElement.Q<Label>();
            label.text = message;
        }

        public LogMessage(string message)
        {
            this.message = message;
        }
    }
}
