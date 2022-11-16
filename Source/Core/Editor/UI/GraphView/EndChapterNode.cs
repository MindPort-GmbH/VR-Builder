using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    public class EndChapterNode : StepGraphNode
    {
        public EndChapterNode(IStep step) : base(step)
        {
            titleButtonContainer.Clear();
        }
    }
}
