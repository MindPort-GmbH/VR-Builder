using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UndoRedo;
using VRBuilder.Core.Entities.Factories;

namespace VRBuilder.Core.Editor.UI.GraphView.Nodes
{
    /// <summary>
    /// Step node in a graph view editor.
    /// </summary>
    public class StepGraphNode : ProcessGraphNode
    {
        protected IStep step;
        private readonly Label descriptionLabel;
        private UnityEditor.Experimental.GraphView.GraphView ownerGraph;
        private TextField descriptionEditor;
        private const float descriptionZoomThreshold = 0.75f;

        /// <inheritdoc/>
        public override string Name { get => step.Data.Name; set => step.Data.SetName(value); }

        /// <inheritdoc/>
        public override IStep EntryPoint => step;

        /// <inheritdoc/>
        public override IStep[] Outputs => step.Data.Transitions.Data.Transitions.Select(t => t.Data.TargetStepReference.Entity).ToArray();

        /// <inheritdoc/>
        public override Vector2 Position { get => step.StepMetadata.Position; set => step.StepMetadata.Position = value; }

        public StepGraphNode(IStep step) : base()
        {
            title = step.Data.Name;
            this.step = step;

            descriptionLabel = new Label();
            descriptionLabel.enableRichText = false;
            descriptionLabel.pickingMode = PickingMode.Position;
            descriptionLabel.AddToClassList("step-description");
            // The description must follow the node width without contributing its unwrapped text width.
            mainContainer.RegisterCallback<GeometryChangedEvent>(evt =>
                descriptionLabel.style.maxWidth = evt.newRect.width);
            descriptionLabel.RegisterCallback<GeometryChangedEvent>(_ => UpdateDescriptionPreview());
            descriptionLabel.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 2 && evt.button == 0)
                {
                    EditDescription();
                    evt.StopImmediatePropagation();
                }
            }, TrickleDown.TrickleDown);
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                ownerGraph = GetFirstAncestorOfType<UnityEditor.Experimental.GraphView.GraphView>();
                if (ownerGraph != null) ownerGraph.viewTransformChanged += OnGraphTransformChanged;
                RefreshDescription();
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                if (ownerGraph != null) ownerGraph.viewTransformChanged -= OnGraphTransformChanged;
                ownerGraph = null;
            });
            mainContainer.Insert(mainContainer.IndexOf(titleContainer) + 1, descriptionLabel);
            RefreshDescription();

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
            style.height = StyleKeyword.Auto;
            RefreshExpandedState();
            RefreshPorts();
        }

        /// <inheritdoc/>
        public override void Refresh()
        {
            title = step.Data.Name;
            RefreshDescription();
            base.Refresh();
        }

        private void RefreshDescription()
        {
            descriptionLabel.tooltip = step.Data.Description;
            descriptionLabel.style.display = descriptionEditor != null ||
                string.IsNullOrWhiteSpace(step.Data.Description) ||
                (ownerGraph != null && ownerGraph.contentViewContainer.resolvedStyle.scale.value.x < descriptionZoomThreshold)
                ? DisplayStyle.None
                : DisplayStyle.Flex;
            UpdateDescriptionPreview();
        }

        private void OnGraphTransformChanged(UnityEditor.Experimental.GraphView.GraphView graph)
        {
            RefreshDescription();
        }

        private void UpdateDescriptionPreview()
        {
            string text = step.Data.Description ?? string.Empty;
            float width = descriptionLabel.contentRect.width;
            if (width <= 0) return;

            float height = descriptionLabel.MeasureTextSize("Ag\nAg", width,
                VisualElement.MeasureMode.AtMost, 0, VisualElement.MeasureMode.Undefined).y;
            bool Fits(string value) => descriptionLabel.MeasureTextSize(value, width,
                VisualElement.MeasureMode.AtMost, 0, VisualElement.MeasureMode.Undefined).y <= height + 0.01f;

            if (!Fits(text))
            {
                int low = 0;
                int high = text.Length;
                while (low < high)
                {
                    int middle = (low + high + 1) / 2;
                    if (Fits(text.Substring(0, middle).TrimEnd() + "...")) low = middle;
                    else high = middle - 1;
                }
                if (low > 0 && char.IsHighSurrogate(text[low - 1])) low--;
                text = text.Substring(0, low).TrimEnd() + "...";
            }
            descriptionLabel.text = text;
        }

        private void EditDescription()
        {
            if (descriptionEditor != null) return;
            string oldValue = step.Data.Description;
            TextField field = new TextField { value = oldValue, multiline = true };
            descriptionEditor = field;
            field.style.width = descriptionLabel.layout.width;
            field.style.whiteSpace = WhiteSpace.Normal;
            mainContainer.Insert(mainContainer.IndexOf(descriptionLabel) + 1, field);
            RefreshDescription();

            void Finish(bool save)
            {
                if (descriptionEditor != field) return;
                string newValue = field.value;
                descriptionEditor = null;
                field.RemoveFromHierarchy();
                if (save && newValue != oldValue)
                {
                    RevertableChangesHandler.Do(new ProcessCommand(
                        () => { step.Data.Description = newValue; RefreshDescription(); },
                        () => { step.Data.Description = oldValue; RefreshDescription(); }));
                }
                RefreshDescription();
            }

            field.RegisterCallback<FocusOutEvent>(_ => Finish(true));
            field.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Escape || (evt.keyCode == KeyCode.Return && (evt.ctrlKey || evt.commandKey)))
                {
                    Finish(evt.keyCode != KeyCode.Escape);
                    evt.StopImmediatePropagation();
                }
            }, TrickleDown.TrickleDown);
            field.Focus();
            field.SelectAll();
        }

        /// <summary>
        /// Creates a transition port supporting undo.
        /// </summary>
        protected virtual void CreatePortWithUndo()
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

        /// <summary>
        /// Removes the specified output port.
        /// </summary>        
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

        /// <summary>
        /// Removes the specified output port supporting undo.
        /// </summary>        
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

        /// <inheritdoc/>
        public override void OnSelected()
        {
            base.OnSelected();

            GlobalEditorHandler.ChangeCurrentStep(step);
            GlobalEditorHandler.StartEditingStep();
        }

        /// <inheritdoc/>
        public override void SetOutput(int index, IStep output)
        {
            step.Data.Transitions.Data.Transitions[index].Data.TargetStepReference.Set(output);
        }

        /// <inheritdoc/>
        public override void AddToChapter(IChapter chapter)
        {
            chapter.Data.Steps.Add(step);
        }

        /// <inheritdoc/>
        public override void RemoveFromChapter(IChapter chapter)
        {
            if (chapter.ChapterMetadata.LastSelectedStep == step)
            {
                chapter.ChapterMetadata.LastSelectedStep = null;
                GlobalEditorHandler.ChangeCurrentStep(null);
            }

            chapter.Data.Steps.Remove(step);
        }

        /// <inheritdoc/>
        public override void UpdateOutputPortName(Port outputPort, Node input)
        {
            int index = outputContainer.IndexOf(outputPort);
            if (index >= 0 && string.IsNullOrEmpty(step.Data.Transitions.Data.Transitions[index].Data.Name) == false)
            {
                outputPort.portName = step.Data.Transitions.Data.Transitions[index].Data.Name;
            }
            else
            {
                base.UpdateOutputPortName(outputPort, input);
            }
        }
    }
}
