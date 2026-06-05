using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using VRBuilder.Core;

namespace MindPort.GTKProcessEditor.Editor
{
    // Draws chapter 1 of a process into the graph: a Start node, one Step node per step, and connections.
    public static class ProcessGraphRenderer
    {
        public static int Render(ProcessGraph graph, IProcess process)
        {
            IChapter chapter = process?.Data.FirstChapter;
            if (chapter == null)
            {
                return 0;
            }

            // Clear existing nodes so re-rendering an open graph updates it in place.
            foreach (INode existing in graph.GetNodes().ToList())
            {
                existing.RemoveFromGraph();
            }

            GraphNodeFactory factory = new GraphNodeFactory(graph);
            if (!factory.IsAvailable)
            {
                return 0;
            }

            StartNode start = new StartNode();
            factory.CreateNode(start, chapter.ChapterMetadata.EntryNodePosition);

            Dictionary<IStep, StepNode> nodes = new Dictionary<IStep, StepNode>();
            foreach (IStep step in chapter.Data.Steps)
            {
                StepNode node = new StepNode
                {
                    StepGuid = step.StepMetadata.Guid.ToString(),
                    StepName = step.Data.Name,
                    TransitionLabels = BuildTransitionLabels(step)
                };
                factory.CreateNode(node, step.StepMetadata.Position);
                nodes[step] = node;
            }

            if (chapter.Data.FirstStep != null && nodes.TryGetValue(chapter.Data.FirstStep, out StepNode firstNode))
            {
                factory.Connect(
                    start.GetOutputPortByName(StartNode.OutputPortName),
                    firstNode.GetInputPortByName(StepNode.InputPortName));
            }

            foreach (IStep step in chapter.Data.Steps)
            {
                StepNode sourceNode = nodes[step];
                IList<ITransition> transitions = step.Data.Transitions.Data.Transitions;

                for (int i = 0; i < transitions.Count; i++)
                {
                    IStep target = transitions[i].Data.TargetStep;
                    if (target != null && nodes.TryGetValue(target, out StepNode targetNode))
                    {
                        factory.Connect(
                            sourceNode.GetOutputPortByName(StepNode.GetOutputPortName(i)),
                            targetNode.GetInputPortByName(StepNode.InputPortName));
                    }
                }
            }

            factory.Save();
            return graph.NodeCount;
        }

        private static string[] BuildTransitionLabels(IStep step)
        {
            IList<ITransition> transitions = step.Data.Transitions.Data.Transitions;
            string[] labels = new string[transitions.Count];

            for (int i = 0; i < transitions.Count; i++)
            {
                string name = transitions[i].Data.Name;
                labels[i] = string.IsNullOrEmpty(name) ? $"Transition {i + 1}" : name;
            }

            return labels;
        }
    }
}
