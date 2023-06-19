using VRBuilder.Core.Behaviors;

namespace VRBuilder.Core
{
    /// <summary>
    /// Postprocessing for a step group node.
    /// </summary>
    public class ParallelExecutionPostProcessing : EntityPostProcessing<IStep>
    {
        public override void Execute(IStep entity)
        {
            if (entity.StepMetadata.StepType == "parallelExecution")
            {
                IChapter path1 = EntityFactory.CreateChapter("Path 1");
                IChapter path2 = EntityFactory.CreateChapter("Path 2");

                entity.Data.Behaviors.Data.Behaviors.Add(new ExecuteChaptersBehavior(new[] { path1, path2 }));

                entity.Data.Transitions.Data.Transitions.Add(EntityFactory.CreateTransition());
            }
        }
    }
}
