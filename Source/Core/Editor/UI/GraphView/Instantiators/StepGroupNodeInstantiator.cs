using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI.GraphView.Nodes;

namespace VRBuilder.Core.Editor.UI.GraphView.Instantiators
{
    /// <summary>
    /// Instantiator for the Step Group node.
    /// </summary>
    public class StepGroupNodeInstantiator : IStepNodeInstantiator
    {
        /// <inheritdoc/>
        public string Name => "Step Group";

        /// <inheritdoc/>
        public bool IsInNodeMenu => true;

        /// <inheritdoc/>
        public int Priority => 100;

        /// <inheritdoc/>
        public string StepType => "stepGroup";

        /// <inheritdoc/>
        public ProcessGraphNode InstantiateNode(IStep step)
        {
            return new StepGroupNode(step);
        }

        /// <inheritdoc/>
        public DropdownMenuAction.Status GetContextMenuStatus(IEventHandler target, IChapter currentChapter)
        {
            return DropdownMenuAction.Status.Normal;
        }
    }
}
