using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    public class StepGroupNodeInstantiator : IStepNodeInstantiator
    {
        public string Name => "Step Group";

        public bool IsInNodeMenu => true;

        public int Priority => 200;

        public string StepType => "stepGroup";

        public ProcessGraphNode InstantiateNode(IStep step)
        {
            return new StepGroupNode(step);
        }
    }
}
