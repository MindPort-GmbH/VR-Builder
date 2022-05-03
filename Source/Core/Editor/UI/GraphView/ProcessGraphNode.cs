using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core;
using VRBuilder.Editor.UndoRedo;

namespace VRBuilder.Editor.UI.Graphics
{
    /// <summary>
    /// Step node in a graph view editor.
    /// </summary>
    public class ProcessGraphNode : Node
    {
        private Label label;
        private static EditorIcon deleteIcon;
        private Vector2 defaultNodeSize = new Vector2(200, 300);

        /// <summary>
        /// True if this is the "Start" node.
        /// </summary>
        public bool IsEntryPoint { get; set; }

        /// <summary>
        /// Step stored in this node.
        /// </summary>
        public IStep Step { get; set; }

        public ProcessGraphNode() : base()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("ProcessGraphNode"));

            label = titleContainer.Q<Label>();
            label.RegisterCallback<MouseDownEvent>(e => OnMouseDownEvent(e));

            titleContainer.style.backgroundColor = new StyleColor(new Color32(38, 144, 119, 192));
        }

        public ProcessGraphNode(IStep step) : this()
        {

            title = step.Data.Name;
            Step = step;

            Port inputPort = CreatePort(Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "";
            inputContainer.Add(inputPort);

            foreach (ITransition transition in step.Data.Transitions.Data.Transitions)
            {
                Port outputPort = AddTransitionPort();
            }

            Button addTransitionButton = new Button(() => { CreatePortWithUndo(); });
            addTransitionButton.text = "+";
            titleButtonContainer.Clear();
            titleButtonContainer.Add(addTransitionButton);

            capabilities |= Capabilities.Renamable;

            SetPosition(new Rect(Step.StepMetadata.Position, defaultNodeSize));
            RefreshExpandedState();
            RefreshPorts();
        }

        private Image CreateDeleteTransitionIcon()
        {
            if (deleteIcon == null)
            {
                deleteIcon = new EditorIcon("icon_delete");
            }

            Image icon = new Image();
            icon.image = deleteIcon.Texture;
            icon.style.paddingBottom = 2;
            icon.style.paddingLeft = 2;
            icon.style.paddingRight = 2;
            icon.style.paddingTop = 2;

            return icon;
        }

        private Port CreatePort(Direction direction, Port.Capacity capacity = Port.Capacity.Single)
        {
            return InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(ProcessExec));
        }

        internal void CreatePortWithUndo()
        {
            ITransition transition = EntityFactory.CreateTransition();

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    Step.Data.Transitions.Data.Transitions.Add(transition);
                    AddTransitionPort();
                },
                () =>
                {
                    RemovePort(outputContainer[Step.Data.Transitions.Data.Transitions.IndexOf(transition)] as Port);
                }
            ));
        }

        public Port AddTransitionPort(bool isDeletablePort = true, int index = -1)
        {
            Port port = CreatePort(Direction.Output);

            if (isDeletablePort)
            {
                Button deleteButton = new Button(() => RemovePortWithUndo(port));

                Image icon = CreateDeleteTransitionIcon();
                deleteButton.Add(icon);
                icon.StretchToParentSize();

                deleteButton.style.alignSelf = Align.Stretch;

                port.contentContainer.Insert(1, deleteButton);
            }

            UpdateOutputPortName(port, null);

            if (index < 0)
            {
                outputContainer.Add(port);
            }
            else
            {
                outputContainer.Insert(index, port);
            }

            RefreshExpandedState();
            RefreshPorts();

            return port;
        }

        public void UpdateOutputPortName(Port outputPort, Node input)
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

        private void RemovePort(Port port)
        {
            Edge edge = port.connections.FirstOrDefault();

            if (edge != null)
            {
                edge.input.Disconnect(edge);                
                edge.parent.Remove(edge);
            }

            int index = outputContainer.IndexOf(port);
            Step.Data.Transitions.Data.Transitions.RemoveAt(index);

            outputContainer.Remove(port);

            if (outputContainer.childCount == 0)
            {
                CreatePortWithUndo();
            }

            RefreshPorts();
            RefreshExpandedState();
        }

        private void RemovePortWithUndo(Port port)
        {
            int index = outputContainer.IndexOf(port);
            ITransition removedTransition = Step.Data.Transitions.Data.Transitions[index];
            //IChapter storedChapter = currentChapter;

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    RemovePort(port);
                },
                () =>
                {
                    Step.Data.Transitions.Data.Transitions.Insert(index, removedTransition);
                    AddTransitionPort(true, index);
                    //SetChapter(storedChapter);
                }
            ));
        }

        void OnMouseDownEvent(MouseDownEvent e)
        {
            if ((e.clickCount == 2) && e.button == (int)MouseButton.LeftMouse && IsRenamable())
            {
                OpenTextEditor();
                e.PreventDefault();
                e.StopImmediatePropagation();
            }
        }

        private void OpenTextEditor()
        {
            label.text = "";
            TextField textField = new TextField();
            textField.value = Step.Data.Name;

            textField.RegisterCallback<FocusOutEvent>(e => OnEditTextFinished(textField));
            label.Add(textField);

            textField.Focus();
            textField.SelectAll();
        }

        private void OnEditTextFinished(TextField textField)
        {
            Step.Data.Name = textField.value;
            label.text = textField.value;
            label.Remove(textField);            
        }

        public override void OnSelected()
        {
            base.OnSelected();

            GlobalEditorHandler.ChangeCurrentStep(Step);
            GlobalEditorHandler.StartEditingStep();
        }  
    }
}
