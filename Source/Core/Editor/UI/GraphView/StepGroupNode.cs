using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;
using VRBuilder.Editor.UndoRedo;

namespace VRBuilder.Editor.UI.Graphics
{
    public class StepGroupNode : StepGraphNode, IContextMenuActions
    {
        private ExecuteChapterBehavior behavior;
        protected ExecuteChapterBehavior Behavior
        {
            get
            {
                if (behavior == null)
                {
                    behavior = (ExecuteChapterBehavior)step.Data.Behaviors.Data.Behaviors.FirstOrDefault(behavior => behavior is ExecuteChapterBehavior);
                }

                return behavior;
            }
        }

        public StepGroupNode(IStep step) : base(step)
        {
            titleButtonContainer.Clear();
            extensionContainer.style.backgroundColor = new Color(.2f, .2f, .2f, .8f);
            DrawButtons();
            RefreshExpandedState();
            RegisterCallback<MouseDownEvent>(e => OnNodeClicked(e));
        }

        private void OnNodeClicked(MouseDownEvent e)
        {
            if ((e.clickCount == 2) && e.button == (int)MouseButton.LeftMouse && IsRenamable())
            {
                ExpandNode();
                e.PreventDefault();
                e.StopImmediatePropagation();
            }
        }

        private void DrawButtons()
        {
            Button expandButton = new Button(() => ExpandNode());
            expandButton.text = "Expand";
            extensionContainer.Add(expandButton);
        }

        private void ExplodeNode()
        {
            IChapter currentChapter = GlobalEditorHandler.GetCurrentChapter();
            IEnumerable<ITransition> leadingTransitions = new List<ITransition>(currentChapter.Data.Steps.SelectMany(step => step.Data.Transitions.Data.Transitions).Where(transition => transition.Data.TargetStep == step));

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    ExplodeGroup(currentChapter, leadingTransitions);
                    GlobalEditorHandler.RequestNewChapter(currentChapter);
                },
                () =>
                {
                    UndoExplodeGroup(currentChapter, leadingTransitions);
                    GlobalEditorHandler.RequestNewChapter(currentChapter);
                }
            ));
        }

        private void ExplodeGroup(IChapter currentChapter, IEnumerable<ITransition> leadingTransitions)
        {
            foreach (IStep addedStep in Behavior.Data.Chapter.Data.Steps)
            {
                currentChapter.Data.Steps.Add(addedStep);
                addedStep.StepMetadata.Position = step.StepMetadata.Position;
            }

            foreach (ITransition transition in leadingTransitions)
            {
                transition.Data.TargetStep = Behavior.Data.Chapter.Data.FirstStep;
            }

            if (currentChapter.Data.FirstStep == step)
            {
                currentChapter.Data.FirstStep = Behavior.Data.Chapter.Data.FirstStep;
            }

            currentChapter.Data.Steps.Remove(step);
        }

        private void UndoExplodeGroup(IChapter currentChapter, IEnumerable<ITransition> leadingTransitions)
        {
            currentChapter.Data.Steps.Add(step);

            foreach (ITransition transition in leadingTransitions)
            {
                transition.Data.TargetStep = step;
            }

            if(Behavior.Data.Chapter.Data.Steps.Contains(currentChapter.Data.FirstStep))
            {
                currentChapter.Data.FirstStep = step;
            }

            foreach (IStep addedStep in Behavior.Data.Chapter.Data.Steps)
            {
                currentChapter.Data.Steps.Remove(addedStep);
            }
        }

        public void GroupSteps(IChapter currentChapter, IEnumerable<IStep> steps)
        {
            IEnumerable<ITransition> leadingTransitions = currentChapter.Data.Steps.SelectMany(step => step.Data.Transitions.Data.Transitions).Where(transition => steps.Contains(transition.Data.TargetStep));
        }

        private void ExpandNode()
        {
            IChapter currentChapter = GlobalEditorHandler.GetCurrentChapter();

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    GlobalEditorHandler.RequestNewChapter(Behavior.Data.Chapter);
                },
                () =>
                {
                    GlobalEditorHandler.RequestNewChapter(currentChapter);
                }
            ));
        }

        public override void OnSelected()
        {
            base.OnSelected();

            GlobalEditorHandler.ChangeCurrentStep(null);
        }

        protected override void OnEditTextFinished(TextField textField)
        {
            Behavior.Data.Chapter.Data.Name = textField.value;
            base.OnEditTextFinished(textField);            
        }

        public void AddContextMenuActions(DropdownMenu menu)
        {
            menu.AppendAction($"Expand", (status) =>
            {
                ExpandNode();
            });

            menu.AppendAction($"Ungroup", (status) =>
            {
                ExplodeNode();                
            });            
        }
    }
}
