using UnityEngine.UIElements;
using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    public class StepGroupNodeInstantiator : IStepNodeInstantiator
    {
        public string Name => "Step Group";

        public bool IsInNodeMenu => true;

        public int Priority => 100;

        public string StepType => "stepGroup";

        public ProcessGraphNode InstantiateNode(IStep step)
        {
            return new StepGroupNode(step);
        }

        /// <inheritdoc/>
        public DropdownMenuAction.Status GetContextMenuStatus(IEventHandler target, IChapter currentChapter)
        {
            if (GlobalEditorHandler.GetCurrentProcess().Data.Chapters.Contains(currentChapter))
            {
                return DropdownMenuAction.Status.Normal;
            }
            else
            {
                return DropdownMenuAction.Status.Disabled;
            }
        }
    }
}
