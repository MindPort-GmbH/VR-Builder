using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core;
using VRBuilder.Editor.Configuration;
using VRBuilder.Editor.UndoRedo;

namespace VRBuilder.Editor.UI.Graphics
{
    /// <summary>
    /// Graphical representation of a process chapter.
    /// </summary>
    public class ProcessGraphView : GraphView
    {
        private Vector2 pasteOffset = new Vector2(-20, -20);
        private IChapter currentChapter;
        private ProcessGraphNode entryNode;
        private int pasteCounter = 0;

        public ProcessGraphView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("ProcessGraph"));

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());            

            GridBackground grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            viewTransform.position = new Vector2(400, 400);

            graphViewChanged = OnGraphChanged;
            serializeGraphElements = OnElementsSerialized;
            unserializeAndPaste = OnElementsPasted;            
        }

        private void OnElementsPasted(string operationName, string data)
        {
            IProcess clipboardProcess = EditorConfigurator.Instance.Serializer.ProcessFromByteArray(Encoding.UTF8.GetBytes(data));
            IChapter storedChapter = currentChapter;
            pasteCounter++;

            RevertableChangesHandler.Do(new ProcessCommand(
            () =>
            {                
                ClearSelection();

                foreach (IStep step in clipboardProcess.Data.FirstChapter.Data.Steps)
                {
                    step.StepMetadata.Position += pasteOffset * pasteCounter;
                    currentChapter.Data.Steps.Add(step);
                }

                IDictionary<IStep, ProcessGraphNode> steps = SetupSteps(clipboardProcess.Data.FirstChapter);
                SetupTransitions(clipboardProcess.Data.FirstChapter, steps);

                foreach (ProcessGraphNode step in steps.Values)
                {
                    AddToSelection(step);
                }                
            },
            () =>
            {
                foreach (IStep step in clipboardProcess.Data.FirstChapter.Data.Steps)
                {
                    SetChapter(storedChapter);
                    DeleteStep(step);
                    SetChapter(currentChapter);
                }
            }
            ));
        }

        private string OnElementsSerialized(IEnumerable<GraphElement> elements)
        {
            pasteCounter = 0;
            IProcess clipboardProcess = EntityFactory.CreateProcess("Clipboard Process");

            foreach(GraphElement element in elements)
            {
                ProcessGraphNode node = element as ProcessGraphNode;
                if (node != null)
                {
                    //TODO
                    clipboardProcess.Data.FirstChapter.Data.Steps.Add(node.EntryPoint);
                }
            }

            byte[] bytes = EditorConfigurator.Instance.Serializer.ProcessToByteArray(clipboardProcess);
            return Encoding.UTF8.GetString(bytes);
        }

        private GraphViewChange OnGraphChanged(GraphViewChange change)
        {            
            if (change.elementsToRemove != null)
            {
                IEnumerable<Edge> removedEdges = change.elementsToRemove.Where(e => e is Edge).Select(e => e as Edge);
                IEnumerable<ProcessGraphNode> removedNodes = change.elementsToRemove.Where(e => e is ProcessGraphNode).Select(e => e as ProcessGraphNode);
                Dictionary<Edge, List<Port>> storedEdgeIO = new Dictionary<Edge, List<Port>>();
                Dictionary<ProcessGraphNode, List<ITransition>> incomingTransitions = new Dictionary<ProcessGraphNode, List<ITransition>>();
                IChapter storedChapter = currentChapter;

                foreach(ProcessGraphNode node in removedNodes)
                {
                    incomingTransitions.Add(node, currentChapter.Data.Steps.SelectMany(s => s.Data.Transitions.Data.Transitions).Where(transition => transition.Data.TargetStep == node.EntryPoint).ToList());
                }

                foreach(Edge edge in removedEdges)
                {
                    List<Port> nodes = new List<Port>() { edge.output, edge.input };
                    storedEdgeIO.Add(edge, nodes);                   
                }

                RevertableChangesHandler.Do(new ProcessCommand(
                    () =>
                    {
                        foreach (Edge edge in removedEdges)
                        {
                            ProcessGraphNode node = edge.output.node as ProcessGraphNode;

                            if (node == null)
                            {
                                continue;
                            }

                            if (node.IsEntryPoint)
                            {
                                currentChapter.Data.FirstStep = null;
                            }
                            else
                            {
                                node.SetOutput(node.outputContainer.IndexOf(edge.output), null);                                
                            }

                            node.UpdateOutputPortName(edge.output, null);
                        }

                        foreach (ProcessGraphNode node in removedNodes)
                        {
                            foreach (ITransition transition in incomingTransitions[node])
                            {
                                transition.Data.TargetStep = null;
                            }

                            node.RemoveFromChapter(currentChapter);
                            SetChapter(currentChapter);
                        }
                    },
                    () =>
                    {
                        SetChapter(storedChapter);

                        foreach (ProcessGraphNode node in removedNodes)
                        {
                            node.AddToChapter(storedChapter);
                            AddElement(node);
                            //storedChapter.Data.Steps.Add(node.Step);
                            //ProcessGraphNode newNode = CreateStepNode(node.Step);

                            foreach (ITransition transition in incomingTransitions[node])
                            {
                                transition.Data.TargetStep = node.EntryPoint;
                            }
                        }

                        foreach (Edge edge in removedEdges)
                        {
                            Port outputPort = storedEdgeIO[edge][0];
                            ProcessGraphNode output = outputPort.node as ProcessGraphNode;
                            ProcessGraphNode input = storedEdgeIO[edge][1].node as ProcessGraphNode;

                            if (output == null || input == null)
                            {
                                continue;
                            }

                            if (output.IsEntryPoint)
                            {
                                storedChapter.Data.FirstStep = input.EntryPoint;
                            }
                            else
                            {
                                IStep targetStep = output.Outputs[output.outputContainer.IndexOf(outputPort)];

                                if (targetStep == null)
                                {
                                    output.SetOutput(output.outputContainer.IndexOf(outputPort), input.EntryPoint);
                                }
                            }

                            ((ProcessGraphNode)outputPort.node).UpdateOutputPortName(outputPort, input);
                            SetChapter(currentChapter);
                        }
                    }
                    ));
            }

            if(change.movedElements != null)
            {
                IEnumerable<ProcessGraphNode> movedNodes = change.movedElements.Where(e => e is ProcessGraphNode).Select(e => e as ProcessGraphNode);
                Dictionary<ProcessGraphNode, Vector2> storedPositions = new Dictionary<ProcessGraphNode, Vector2>();
                IChapter storedChapter = currentChapter;

                foreach (ProcessGraphNode node in movedNodes)
                {
                    storedPositions.Add(node, node.Position);
                }

                RevertableChangesHandler.Do(new ProcessCommand(
                    () =>
                    {
                        foreach(ProcessGraphNode node in movedNodes)
                        {
                            if (node.IsEntryPoint)
                            {
                                currentChapter.ChapterMetadata.EntryNodePosition = (node).GetPosition().position;
                            }
                            else
                            {
                                node.Position = node.GetPosition().position;
                            }
                        }
                    },
                    () =>
                    {
                        foreach (ProcessGraphNode node in storedPositions.Keys)
                        {
                            node.SetPosition(new Rect(storedPositions[node], node.contentRect.size));

                            if (node.IsEntryPoint)
                            {
                                storedChapter.ChapterMetadata.EntryNodePosition = storedPositions[node];
                            }
                            else
                            {
                                node.Position = storedPositions[node];
                            }                          
                        }

                        if (storedChapter != currentChapter)
                        {
                            SetChapter(storedChapter);
                        }
                    }
                    ));
            }

            if (change.edgesToCreate != null)
            {
                foreach (Edge edge in change.edgesToCreate)
                {
                    CreateEdgeWithUndo(edge);
                }
            }

            return change;
        }

        private void CreateEdgeWithUndo(Edge edge)
        {
            ProcessGraphNode targetNode = edge.input.node as ProcessGraphNode;

            if (targetNode == null)
            {
                Debug.LogError("Connected non-step node");
                return;
            }

            ProcessGraphNode startNode = edge.output.node as ProcessGraphNode;

            if (startNode == null)
            {
                Debug.LogError("Connected non-step node");
                return;
            }

            IChapter storedChapter = currentChapter;

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    if (startNode.IsEntryPoint)
                    {
                        currentChapter.Data.FirstStep = targetNode.EntryPoint;
                        ((ProcessGraphNode)edge.output.node).UpdateOutputPortName(edge.output, targetNode);
                    }
                    else
                    {
                        //ITransition transition = startNode.Step.Data.Transitions.Data.Transitions[startNode.outputContainer.IndexOf(edge.output)];
                        //IStep targetStep = startNode.Outputs[startNode.outputContainer.IndexOf(edge.output)];
                        startNode.SetOutput(startNode.outputContainer.IndexOf(edge.output), targetNode.EntryPoint);
                        //transition.Data.TargetStep = targetNode.EntryPoint;
                        ((ProcessGraphNode)edge.output.node).UpdateOutputPortName(edge.output, targetNode);
                    }
                },
                () =>
                {
                    if (startNode.IsEntryPoint)
                    {
                        storedChapter.Data.FirstStep = null;
                        ((ProcessGraphNode)edge.output.node).UpdateOutputPortName(edge.output, null);
                    }
                    else
                    {
                        //ITransition transition = startNode.Step.Data.Transitions.Data.Transitions[startNode.outputContainer.IndexOf(edge.output)];
                        startNode.SetOutput(startNode.outputContainer.IndexOf(edge.output), null);
                        //transition.Data.TargetStep = null;
                        ((ProcessGraphNode)edge.output.node).UpdateOutputPortName(edge.output, null);
                    }

                    RemoveElement(edge);

                    if(currentChapter != storedChapter)
                    {
                        SetChapter(storedChapter);
                    }
                }
                ));
        }

        private void DeleteStep(IStep step)
        {
            if (currentChapter.ChapterMetadata.LastSelectedStep == step)
            {
                currentChapter.ChapterMetadata.LastSelectedStep = null;
                GlobalEditorHandler.ChangeCurrentStep(null);
            }

            currentChapter.Data.Steps.Remove(step);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {            
            evt.menu.AppendAction($"Create Step Node", (status) => {
                IStep step = EntityFactory.CreateStep("New Step");
                step.StepMetadata.Position = contentViewContainer.WorldToLocal(status.eventInfo.mousePosition);
                currentChapter.Data.Steps.Add(step);
                CreateStepNodeWithUndo(step);
                GlobalEditorHandler.CurrentStepModified(step);
            });

            evt.menu.AppendSeparator();

            base.BuildContextualMenu(evt);
        }

        public void SetChapter(IChapter chapter)
        {
            if (chapter != GlobalEditorHandler.GetCurrentChapter())
            {
                GlobalEditorHandler.SetCurrentChapter(chapter);
            }

            currentChapter = chapter;                        

            nodes.ForEach(RemoveElement);
            edges.ForEach(RemoveElement);

            entryNode = new EntryPointNode();
            AddElement(entryNode);

            IDictionary<IStep, ProcessGraphNode> stepNodes = SetupSteps(currentChapter);

            foreach (IStep step in stepNodes.Keys)
            {
                ProcessGraphNode node = stepNodes[step];
                AddElement(node);
            }

            SetupTransitions(currentChapter, stepNodes);
        }      

        private void LinkNodes(Port output, Port input)
        {
            Edge edge = new Edge
            {
                output = output,
                input = input,
            };

            edge.input.Connect(edge);
            edge.output.Connect(edge);
            Add(edge);

            ((ProcessGraphNode)output.node).UpdateOutputPortName(output, input.node);
        }

        private IDictionary<IStep, ProcessGraphNode> SetupSteps(IChapter chapter)
        {
            return chapter.Data.Steps.OrderBy(step => step == chapter.ChapterMetadata.LastSelectedStep).ToDictionary(step => step, CreateStepNode);
        }

        private void SetupTransitions(IChapter chapter, IDictionary<IStep, ProcessGraphNode> stepNodes)
        {
            if (chapter.Data.FirstStep != null)
            {
                LinkNodes(entryNode.outputContainer[0].Query<Port>(), stepNodes[chapter.Data.FirstStep].inputContainer[0].Query<Port>());
            }

            foreach (IStep step in stepNodes.Keys)
            {
                foreach (ITransition transition in step.Data.Transitions.Data.Transitions)
                {
                    Port outputPort = stepNodes[step].outputContainer[step.Data.Transitions.Data.Transitions.IndexOf(transition)] as Port;

                    if (transition.Data.TargetStep != null && outputPort != null)
                    {
                        ProcessGraphNode target = stepNodes[transition.Data.TargetStep];
                        LinkNodes(outputPort, target.inputContainer[0].Query<Port>());
                    }
                }
            }
        }        

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }

        internal void CreateStepNodeWithUndo(IStep step)
        {
            IChapter storedChapter = currentChapter;

            RevertableChangesHandler.Do(new ProcessCommand(
            () =>
            {
                CreateStepNode(step);                
            },
            () =>
            {
                DeleteStep(step);
                SetChapter(storedChapter);
            }
            ));
        }

        internal ProcessGraphNode CreateStepNode(IStep step)
        {
            StepGraphNode node = new StepGraphNode(step);
            AddElement(node);
            return node;
        }
    }
}
