using UnityEditor.Experimental.GraphView;
using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    public class ProcessGraphNode : Node
    {        
        public override void OnSelected()
        {
            base.OnSelected();

            GlobalEditorHandler.ChangeCurrentStep(Step);
            GlobalEditorHandler.StartEditingStep();
        }

        public bool IsEntryPoint { get; set; }

        public IStep Step { get; set; }        
    }
}
