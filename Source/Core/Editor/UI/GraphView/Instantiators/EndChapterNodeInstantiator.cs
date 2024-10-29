using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI.GraphView.Nodes;

namespace VRBuilder.Core.Editor.UI.GraphView.Instantiators
{
    /// <summary>
    /// Instantiator for the End Chapter node.
    /// </summary>
    public class EndChapterNodeInstantiator : IStepNodeInstantiator
    {
        /// <inheritdoc/>
        public string Name => "End Chapter";

        /// <inheritdoc/>
        public bool IsInNodeMenu => true;

        /// <inheritdoc/>
        public int Priority => 150;

        /// <inheritdoc/>
        public string StepType => "endChapter";

        /// <inheritdoc/>
        public DropdownMenuAction.Status GetContextMenuStatus(IEventHandler target, IChapter currentChapter)
        {
            return DropdownMenuAction.Status.Normal;
        }

        /// <inheritdoc/>
        public ProcessGraphNode InstantiateNode(IStep step)
        {
            return new EndChapterNode(step);
        }
    }
}
