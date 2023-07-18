using UnityEngine.UIElements;
using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    /// <summary>
    /// Instantiator for the Step Group node.
    /// </summary>
    public class ParallelExecutionNodeInstantiator : IStepNodeInstantiator
    {
        /// <inheritdoc/>
        public string Name => "Parallel Execution";

        /// <inheritdoc/>
        public bool IsInNodeMenu => true;

        /// <inheritdoc/>
        public int Priority => 100;

        /// <inheritdoc/>
        public string StepType => "parallelExecution";

        /// <inheritdoc/>
        public ProcessGraphNode InstantiateNode(IStep step)
        {
            return new ParallelExecutionNode(step);
        }

        /// <inheritdoc/>
        public DropdownMenuAction.Status GetContextMenuStatus(IEventHandler target, IChapter currentChapter)
        {
            return DropdownMenuAction.Status.Normal;
        }
    }
}
