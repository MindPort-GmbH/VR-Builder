using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Entities.Factories;

namespace VRBuilder.Core.Editor.UI.GraphView.Nodes
{
    /// <summary>
    /// <see cref="IStep"/> implementation of <see cref="EntityPostProcessing{T}"/> specific for "endChapter" steps.
    /// </summary>
    public class EndChapterPostProcessing : EntityPostProcessing<IStep>
    {
        /// <inheritdoc />
        public override void Execute(IStep entity)
        {
            if (entity.StepMetadata.StepType == "endChapter")
            {
                entity.Data.Behaviors.Data.Behaviors.Add(new GoToChapterBehavior());
                entity.Data.Transitions.Data.Transitions.Add(EntityFactory.CreateTransition());
            }
        }
    }
}
