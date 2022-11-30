using VRBuilder.Core;

namespace VRBuilder.Editor.UI.Graphics
{
    /// <summary>
    /// Instantiator for the End Chapter node.
    /// </summary>
    public class EndChapterNodeInstantiator : IStepNodeInstantiator
    {
        public string Name => "End Chapter";

        public bool IsInNodeMenu => true;

        public int Priority => 150;

        public string StepType => "endChapter";

        public ProcessGraphNode InstantiateNode(IStep step)
        {
            return new EndChapterNode(step);
        }
    }
}
