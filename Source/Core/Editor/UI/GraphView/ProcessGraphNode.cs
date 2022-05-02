using UnityEditor.Experimental.GraphView;
using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    /// <summary>
    /// Step node in a graph view editor.
    /// </summary>
    public class ProcessGraphNode : Node
    {        
        public override void OnSelected()
        {
            base.OnSelected();

            GlobalEditorHandler.ChangeCurrentStep(Step);
            GlobalEditorHandler.StartEditingStep();
        }

        /// <summary>
        /// True if this is the "Start" node.
        /// </summary>
        public bool IsEntryPoint { get; set; }

        /// <summary>
        /// Step stored in this node.
        /// </summary>
        public IStep Step { get; set; }        
    }
}
