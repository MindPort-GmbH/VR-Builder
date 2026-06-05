using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Editor;
using VRBuilder.Core.Editor.ProcessAssets;
using VRBuilder.Core.Entities.Factories;

namespace MindPort.GTKProcessEditor.Editor
{
    // Writes GTK graph edits back to the live VR Builder process and saves its JSON. The graph is
    // authoritative: the process is overwritten to match it, including deletions.
    public static class GraphSync
    {
        private static ProcessGraph pendingGraph;
        private static bool isFlushScheduled;

        public static bool IsSyncing { get; private set; }

        public static void Schedule(ProcessGraph graph)
        {
            if (IsSyncing || graph == null)
            {
                return;
            }

            pendingGraph = graph;
            if (isFlushScheduled)
            {
                return;
            }

            isFlushScheduled = true;
            EditorApplication.delayCall += Flush;
        }

        private static void Flush()
        {
            isFlushScheduled = false;
            ProcessGraph graph = pendingGraph;
            pendingGraph = null;

            if (graph == null)
            {
                return;
            }

            try
            {
                IsSyncing = true;
                Sync(graph);
            }
            finally
            {
                IsSyncing = false;
            }
        }

        private static void Sync(ProcessGraph graph)
        {
            IProcess process = ResolveProcess(graph);
            IChapter chapter = process?.Data.FirstChapter;
            if (process == null || chapter == null)
            {
                return;
            }

            List<StepNode> stepNodes = graph.GetNodes().OfType<StepNode>().ToList();
            StartNode startNode = graph.GetNodes().OfType<StartNode>().FirstOrDefault();

            // No step nodes but the chapter still has steps is likely a transient load state; don't wipe it.
            if (stepNodes.Count == 0 && chapter.Data.Steps.Count > 0)
            {
                return;
            }

            Dictionary<StepNode, IStep> map = EnsureSteps(chapter, stepNodes);
            RemoveOrphans(chapter, map.Values);
            SyncTransitions(map);
            SyncFirstStep(chapter, startNode, map);
            SyncPositions(map, startNode, chapter);

            GlobalEditorHandler.CurrentProcessModified();
            ProcessAssetManager.Save(process);

            // Don't SaveGraph here: re-serializing the open graph mid-interaction corrupts GTK's selection
            // state (its ShouldBeHighlighted NPE). New-node guids only need to live in memory for the session.

            Debug.Log($"[GTK Process Editor] Saved graph → process '{process.Data.Name}' ({chapter.Data.Steps.Count} step(s)).");
        }

        private static IProcess ResolveProcess(ProcessGraph graph)
        {
            if (string.IsNullOrEmpty(graph.ProcessName))
            {
                return null;
            }

            IProcess current = GlobalEditorHandler.GetCurrentProcess();
            if (current != null && current.Data.Name == graph.ProcessName)
            {
                return current;
            }

            GlobalEditorHandler.SetCurrentProcess(graph.ProcessName);
            return GlobalEditorHandler.GetCurrentProcess();
        }

        private static Dictionary<StepNode, IStep> EnsureSteps(IChapter chapter, List<StepNode> stepNodes)
        {
            Dictionary<string, IStep> byGuid = chapter.Data.Steps
                .ToDictionary(step => step.StepMetadata.Guid.ToString(), step => step);

            Dictionary<StepNode, IStep> map = new Dictionary<StepNode, IStep>();

            foreach (StepNode node in stepNodes)
            {
                bool hasGuid = !string.IsNullOrEmpty(node.StepGuid);
                IStep step = null;
                if (hasGuid)
                {
                    byGuid.TryGetValue(node.StepGuid, out step);
                }

                string name = ReadName(node);

                if (step == null)
                {
                    // Has a guid but no matching step = deleted elsewhere; skip (don't resurrect it).
                    if (hasGuid)
                    {
                        continue;
                    }

                    // New node created in GTK: create its step.
                    step = EntityFactory.CreateStep(string.IsNullOrEmpty(name) ? "Step" : name);
                    chapter.Data.Steps.Add(step);
                    node.StepGuid = step.StepMetadata.Guid.ToString();
                }
                else if (!string.IsNullOrEmpty(name) && name != step.Data.Name)
                {
                    step.Data.SetName(name);
                }

                map[node] = step;
            }

            return map;
        }

        private static string ReadName(StepNode node)
        {
            INodeOption option = node.GetNodeOptionByName(StepNode.NameOption);
            if (option != null && option.TryGetValue(out string value))
            {
                return value;
            }

            return null;
        }

        private static void RemoveOrphans(IChapter chapter, IEnumerable<IStep> keptSteps)
        {
            HashSet<IStep> kept = new HashSet<IStep>(keptSteps);
            foreach (IStep step in chapter.Data.Steps.ToList())
            {
                if (!kept.Contains(step))
                {
                    chapter.Data.Steps.Remove(step);
                }
            }
        }

        private static void SyncTransitions(Dictionary<StepNode, IStep> map)
        {
            foreach (KeyValuePair<StepNode, IStep> pair in map)
            {
                List<IPort> outputs = pair.Key.GetOutputPorts().ToList();
                IList<ITransition> transitions = pair.Value.Data.Transitions.Data.Transitions;

                while (transitions.Count < outputs.Count)
                {
                    transitions.Add(EntityFactory.CreateTransition());
                }
                while (transitions.Count > outputs.Count && transitions.Count > 0)
                {
                    transitions.RemoveAt(transitions.Count - 1);
                }

                for (int i = 0; i < outputs.Count; i++)
                {
                    transitions[i].Data.TargetStep = ResolveTarget(outputs[i], map);
                }
            }
        }

        private static void SyncPositions(Dictionary<StepNode, IStep> map, StartNode startNode, IChapter chapter)
        {
            foreach (KeyValuePair<StepNode, IStep> pair in map)
            {
                pair.Value.StepMetadata.Position = pair.Key.Position;
            }

            if (startNode != null)
            {
                chapter.ChapterMetadata.EntryNodePosition = startNode.Position;
            }
        }

        private static void SyncFirstStep(IChapter chapter, StartNode startNode, Dictionary<StepNode, IStep> map)
        {
            if (startNode == null)
            {
                return;
            }

            IPort outputPort = startNode.GetOutputPortByName(StartNode.OutputPortName);
            chapter.Data.FirstStep = ResolveTarget(outputPort, map);
        }

        private static IStep ResolveTarget(IPort outputPort, Dictionary<StepNode, IStep> map)
        {
            if (outputPort == null || !outputPort.IsConnected)
            {
                return null;
            }

            IPort connected = outputPort.FirstConnectedPort;
            if (connected != null && connected.GetNode() is StepNode target && map.TryGetValue(target, out IStep step))
            {
                return step;
            }

            return null;
        }
    }
}
