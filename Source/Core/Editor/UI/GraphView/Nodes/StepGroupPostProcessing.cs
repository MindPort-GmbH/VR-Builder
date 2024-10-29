using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Entities.Factories;

namespace VRBuilder.Core.Editor.UI.GraphView.Nodes
{
    /// <summary>
    /// Postprocessing for a step group node.
    /// </summary>
    public class StepGroupPostProcessing : EntityPostProcessing<IStep>
    {
        public override void Execute(IStep entity)
        {
            if (entity.StepMetadata.StepType == "stepGroup")
            {
                IChapter group = EntityFactory.CreateChapter(entity.Data.Name);
                entity.Data.Behaviors.Data.Behaviors.Add(new ExecuteChapterBehavior(group));

                entity.Data.Transitions.Data.Transitions.Add(EntityFactory.CreateTransition());
            }
        }
    }
}
