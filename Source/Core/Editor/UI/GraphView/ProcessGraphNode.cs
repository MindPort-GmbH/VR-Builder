using UnityEditor.Experimental.GraphView;

namespace VRBuilder.Editor.UI.Graphics
{
    public class ProcessGraphNode : Node
    {
        public string GUID { get; set; }

        public bool IsEntryPoint { get; set; }
    }
}
