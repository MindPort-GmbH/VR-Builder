using System.Collections.Generic;
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
    public class StepGraphNode : ProcessGraphNode
    {
        private IStep step;
        private static EditorIcon deleteIcon;

        public override string Name { get => step.Data.Name; set => step.Data.Name = value; }

        public override IStep EntryPoint => step;

        public override IStep[] Outputs => step.Data.Transitions.Data.Transitions.Select(t => t.Data.TargetStep).ToArray();

        public override Vector2 Position { get => step.StepMetadata.Position; set => step.StepMetadata.Position = value; }

        public StepGraphNode(IStep step) : base()
        {
            title = step.Data.Name;
            this.step = step;

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

            base.SetPosition(new Rect(this.step.StepMetadata.Position, defaultNodeSize));
            RefreshExpandedState();
            RefreshPorts();
        }

        internal void CreatePortWithUndo()
        {
            ITransition transition = EntityFactory.CreateTransition();

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    step.Data.Transitions.Data.Transitions.Add(transition);
                    AddTransitionPort();
                },
                () =>
                {
                    RemovePort(outputContainer[step.Data.Transitions.Data.Transitions.IndexOf(transition)] as Port);
                }
            ));
        }

        protected void RemovePort(Port port)
        {
            Edge edge = port.connections.FirstOrDefault();

            if (edge != null)
            {
                edge.input.Disconnect(edge);
                edge.parent.Remove(edge);
            }

            int index = outputContainer.IndexOf(port);
            step.Data.Transitions.Data.Transitions.RemoveAt(index);

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
            ITransition removedTransition = step.Data.Transitions.Data.Transitions[index];
            IChapter storedChapter = GlobalEditorHandler.GetCurrentChapter();

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    RemovePort(port);
                },
                () =>
                {
                    step.Data.Transitions.Data.Transitions.Insert(index, removedTransition);
                    AddTransitionPort(true, index);
                    GlobalEditorHandler.RequestNewChapter(storedChapter);
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

        public override void OnSelected()
        {
            base.OnSelected();

            GlobalEditorHandler.ChangeCurrentStep(step);
            GlobalEditorHandler.StartEditingStep();
        }

        public override void SetOutput(int index, IStep output)
        {
            step.Data.Transitions.Data.Transitions[index].Data.TargetStep = output;
        }

        public override void AddToChapter(IChapter chapter)
        {
            chapter.Data.Steps.Add(step);
        }
        
        public override void RemoveFromChapter(IChapter chapter)
        {
            if (chapter.ChapterMetadata.LastSelectedStep == step)
            {
                chapter.ChapterMetadata.LastSelectedStep = null;
                GlobalEditorHandler.ChangeCurrentStep(null);
            }

            chapter.Data.Steps.Remove(step);
        }
    }
}
