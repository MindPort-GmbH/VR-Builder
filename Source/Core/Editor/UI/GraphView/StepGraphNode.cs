using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    public class StepGraphNode : ProcessGraphNode
    {
        public override void OnSelected()
        {
            base.OnSelected();

            GlobalEditorHandler.ChangeCurrentStep(Step);
            GlobalEditorHandler.StartEditingStep();
        }

        public IStep Step { get; set; }        
    }
}
