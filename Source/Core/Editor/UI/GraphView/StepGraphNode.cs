using System;
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

        public override void Refresh()
        {
            title = step.Data.Name;
            base.Refresh();
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

        protected override void RemovePortWithUndo(Port port)
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
