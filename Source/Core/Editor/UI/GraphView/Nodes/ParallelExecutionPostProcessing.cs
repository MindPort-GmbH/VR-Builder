using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Entities.Factories;

namespace VRBuilder.Core.Editor.UI.GraphView.Nodes
{
    /// <summary>
    /// Postprocessing for a parallel execution node.
    /// </summary>
    public class ParallelExecutionPostProcessing : EntityPostProcessing<IStep>
    {
        public override void Execute(IStep entity)
        {
            if (entity.StepMetadata.StepType == "parallelExecution")
            {
                IChapter thread1 = EntityFactory.CreateChapter($"{ParallelExecutionNode.DefaultThreadName} 1");
                IChapter thread2 = EntityFactory.CreateChapter($"{ParallelExecutionNode.DefaultThreadName} 2");

                entity.Data.Behaviors.Data.Behaviors.Add(new ExecuteChaptersBehavior(new[] { thread1, thread2 }));

                entity.Data.Transitions.Data.Transitions.Add(EntityFactory.CreateTransition());
            }
        }
    }
}
