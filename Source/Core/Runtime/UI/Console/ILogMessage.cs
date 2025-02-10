using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.UI.Console
{
    public interface ILogMessage
    {
        void Bind(VisualElement messageElement);
    }
}

