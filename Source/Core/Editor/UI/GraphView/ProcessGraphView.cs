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
using static UnityEditor.TypeCache;

namespace VRBuilder.Editor.UI.Graphics
{
    public class ProcessGraphView : GraphView
    {
        private Vector2 defaultNodeSize = new Vector2(200, 300);
        private IChapter currentChapter;
        public ProcessGraphNode EntryNode { get; private set; }

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

            graphViewChanged = OnGraphChanged;
            serializeGraphElements = OnElementsSerialized;
            unserializeAndPaste = OnElementsPasted;
        }

        private void OnElementsPasted(string operationName, string data)
        {
            IProcess clipboardProcess = EditorConfigurator.Instance.Serializer.ProcessFromByteArray(Encoding.UTF8.GetBytes(data));

            RevertableChangesHandler.Do(new ProcessCommand(
            () =>
            {
                foreach (IStep step in clipboardProcess.Data.FirstChapter.Data.Steps)
                {
                    foreach (ITransition transition in step.Data.Transitions.Data.Transitions)
                    {
                        transition.Data.TargetStep = null;
                    }

                    step.StepMetadata.Position += new Vector2(-20, -20);
                    currentChapter.Data.Steps.Add(step);
                    ProcessGraphNode node = CreateStepNode(step);

                    node.RefreshExpandedState();
                    node.RefreshPorts();
                }
            },
            () =>
            {
                foreach (IStep step in clipboardProcess.Data.FirstChapter.Data.Steps)
                {
                    DeleteStep(step);
                    SetChapter(currentChapter);
                }
            }
            ));
        }

        private string OnElementsSerialized(IEnumerable<GraphElement> elements)
        {
            List<IStep> steps = new List<IStep>();

            IProcess clipboardProcess = EntityFactory.CreateProcess("Clipboard Process");

            foreach(GraphElement element in elements)
            {
                ProcessGraphNode node = element as ProcessGraphNode;
                if (node != null)
                {
                    clipboardProcess.Data.FirstChapter.Data.Steps.Add(node.Step);
                }
            }

            byte[] bytes = EditorConfigurator.Instance.Serializer.ProcessToByteArray(clipboardProcess);
            return Encoding.UTF8.GetString(bytes);
        }

        private GraphViewChange OnGraphChanged(GraphViewChange change)
        {            
            if (change.elementsToRemove != null)
            {
                List<Edge> removedEdges = change.elementsToRemove.Where(e => e is Edge).Select(e => e as Edge).ToList();
                List<ProcessGraphNode> removedNodes = change.elementsToRemove.Where(e => e is ProcessGraphNode).Select(e => e as ProcessGraphNode).ToList();
                Dictionary<Edge, List<Port>> storedEdgeIO = new Dictionary<Edge, List<Port>>();
                Dictionary<ProcessGraphNode, List<ITransition>> incomingTransitions = new Dictionary<ProcessGraphNode, List<ITransition>>();

                foreach(ProcessGraphNode node in removedNodes)
                {
                    incomingTransitions.Add(node, currentChapter.Data.Steps.SelectMany(s => s.Data.Transitions.Data.Transitions).Where(transition => transition.Data.TargetStep == node.Step).ToList());
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
                                node.Step.Data.Transitions.Data.Transitions[node.outputContainer.IndexOf(edge.output)].Data.TargetStep = null;
                            }
                        }

                        foreach (ProcessGraphNode node in removedNodes)
                        {
                            foreach (ITransition transition in incomingTransitions[node])
                            {
                                transition.Data.TargetStep = null;
                            }

                            DeleteStep(node.Step);
                        }
                    },
                    () =>
                    {
                        foreach (ProcessGraphNode node in removedNodes)
                        {
                            currentChapter.Data.Steps.Add(node.Step);
                            ProcessGraphNode newNode = CreateStepNode(node.Step);

                            foreach (ITransition transition in incomingTransitions[node])
                            {
                                transition.Data.TargetStep = newNode.Step;
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
                                currentChapter.Data.FirstStep = input.Step;
                                continue;
                            }
                            else
                            {
                                ITransition transition = output.Step.Data.Transitions.Data.Transitions[output.outputContainer.IndexOf(outputPort)];

                                if (transition.Data.TargetStep == null)
                                {
                                    transition.Data.TargetStep = input.Step;
                                }
                            }

                            UpdateOutputPortName(outputPort, input);
                            SetChapter(currentChapter);
                        }
                    }
                    ));
            }

            if(change.movedElements != null)
            {
                foreach(GraphElement element in change.movedElements)
                {
                    ProcessGraphNode node = element as ProcessGraphNode;

                    if(node == null)
                    {
                        continue;
                    }

                    if (node.IsEntryPoint)
                    {
                        currentChapter.ChapterMetadata.EntryNodePosition = (node).GetPosition().position;
                    }
                    else
                    {
                        node.Step.StepMetadata.Position = node.GetPosition().position;
                    }
                }
            }

            if (change.edgesToCreate != null)
            {
                foreach (Edge edge in change.edgesToCreate)
                {
                    CreateTransitionWithUndo(edge);
                }
            }

            return change;
        }

        private void CreateTransitionWithUndo(Edge edge)
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

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    if (startNode.IsEntryPoint)
                    {
                        currentChapter.Data.FirstStep = targetNode.Step;
                        UpdateOutputPortName(edge.output, targetNode);
                    }
                    else
                    {
                        ITransition transition = startNode.Step.Data.Transitions.Data.Transitions[startNode.outputContainer.IndexOf(edge.output)];
                        transition.Data.TargetStep = targetNode.Step;
                        UpdateOutputPortName(edge.output, targetNode);
                    }
                },
                () =>
                {
                    if (startNode.IsEntryPoint)
                    {
                        currentChapter.Data.FirstStep = null;
                        UpdateOutputPortName(edge.output, null);
                    }
                    else
                    {
                        ITransition transition = startNode.Step.Data.Transitions.Data.Transitions[startNode.outputContainer.IndexOf(edge.output)];
                        transition.Data.TargetStep = null;
                        UpdateOutputPortName(edge.output, null);
                    }

                    RemoveElement(edge);
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

        private void DeleteStepWithUndo(IStep step)
        {
            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    DeleteStep(step);
                },
                () =>
                {
                    CreateStepNode(step);
                }
                ));
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {            
            evt.menu.AppendAction($"Create Node/Step", (status) => {
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
            currentChapter = chapter;

            nodes.ForEach(RemoveElement);
            edges.ForEach(RemoveElement);

            EntryNode = CreateEntryPointNode();
            AddElement(EntryNode);

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

            UpdateOutputPortName(output, input.node);
        }

        private IDictionary<IStep, ProcessGraphNode> SetupSteps(IChapter chapter)
        {
            return chapter.Data.Steps.OrderBy(step => step == chapter.ChapterMetadata.LastSelectedStep).ToDictionary(step => step, CreateStepNode);
        }

        private void SetupTransitions(IChapter chapter, IDictionary<IStep, ProcessGraphNode> stepNodes)
        {
            if (chapter.Data.FirstStep != null)
            {
                LinkNodes(EntryNode.outputContainer[0].Query<Port>(), stepNodes[chapter.Data.FirstStep].inputContainer[0].Query<Port>());
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

        private ProcessGraphNode CreateEntryPointNode()
        {
            ProcessGraphNode node = new ProcessGraphNode
            {
                title = "Start",
                IsEntryPoint = true,                
            };

            AddTransitionPort(node, false);

            node.SetPosition(new Rect(currentChapter.ChapterMetadata.EntryNodePosition, new Vector2(100, 150)));
            return node;
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

        public Port AddTransitionPort(ProcessGraphNode node, bool isDeletablePort = true, int index = -1)
        {
            Port port = CreatePort(node, Direction.Output);

            if (isDeletablePort)
            {
                Button deleteButton = new Button(() => RemovePortWithUndo(node, port))
                {
                    text = "X",
                };

                port.contentContainer.Add(deleteButton);
            }

            UpdateOutputPortName(port, null);

            if (index < 0)
            {
                node.outputContainer.Add(port);
            }
            else
            {
                node.outputContainer.Insert(index, port);
            }

            node.RefreshExpandedState();
            node.RefreshPorts();

            return port;
        }

        private void RemovePort(ProcessGraphNode node, Port port)
        {
            Edge edge = port.connections.FirstOrDefault();           

            if (edge != null)
            {
                edge.input.Disconnect(edge);
                RemoveElement(edge);
            }

            int index = node.outputContainer.IndexOf(port);
            node.Step.Data.Transitions.Data.Transitions.RemoveAt(index);

            node.outputContainer.Remove(port);

            if(node.outputContainer.childCount == 0)
            {
                CreateTransition(node);
            }

            node.RefreshPorts();
            node.RefreshExpandedState();
        }

        private void RemovePortWithUndo(ProcessGraphNode node, Port port)
        {
            int index = node.outputContainer.IndexOf(port);
            ITransition removedTransition = node.Step.Data.Transitions.Data.Transitions[index];

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    RemovePort(node, port);
                },
                () =>
                {
                    node.Step.Data.Transitions.Data.Transitions.Insert(index, removedTransition);
                    AddTransitionPort(node, true, index);
                }
            ));
        }

        private void UpdateOutputPortName(Port outputPort, Node input)
        {
            if (input == null)
            {
                outputPort.portName = "End Chapter";
            }
            else
            {
                outputPort.portName = $"To {input.title}";
            }
        }

        internal void CreateTransition(ProcessGraphNode node)
        {
            ITransition transition = EntityFactory.CreateTransition();

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    node.Step.Data.Transitions.Data.Transitions.Add(transition);
                    AddTransitionPort(node);
                },
                () =>
                {
                    RemovePort(node, node.outputContainer[node.Step.Data.Transitions.Data.Transitions.IndexOf(transition)] as Port);
                }
            ));            
        }

        internal void CreateStepNodeWithUndo(IStep step)
        {
            RevertableChangesHandler.Do(new ProcessCommand(
            () =>
            {
                CreateStepNode(step);                
            },
            () =>
            {
                DeleteStep(step);
                SetChapter(currentChapter);
            }
            ));
        }

        internal ProcessGraphNode CreateStepNode(IStep step)
        {
            ProcessGraphNode node = new ProcessGraphNode
            {
                title = step.Data.Name,
                Step = step,
            };

            Port inputPort = CreatePort(node, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            node.inputContainer.Add(inputPort);

            foreach (ITransition transition in step.Data.Transitions.Data.Transitions)
            {
                Port outputPort = AddTransitionPort(node);
            }

            Button addTransitionButton = new Button(() => { CreateTransition(node); });
            addTransitionButton.text = "New Transition";
            node.titleContainer.Add(addTransitionButton);

            node.SetPosition(new Rect(node.Step.StepMetadata.Position, defaultNodeSize));
            node.RefreshExpandedState();
            node.RefreshPorts();

            AddElement(node);

            return node;
        }

        private Port CreatePort(ProcessGraphNode node, Direction direction, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(ProcessExec));
        }
    }
}
