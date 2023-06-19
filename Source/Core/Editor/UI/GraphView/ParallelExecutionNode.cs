using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;
using VRBuilder.Editor.UndoRedo;

namespace VRBuilder.Editor.UI.Graphics
{
    /// <summary>
    /// Graphical representation of a Step Group node.
    /// </summary>
    public class ParallelExecutionNode : StepGraphNode
    {
        private const float explodeScaleFactor = 0.2f;

        private ExecuteChaptersBehavior behavior;
        protected ExecuteChaptersBehavior Behavior
        {
            get
            {
                if (behavior == null)
                {
                    behavior = (ExecuteChaptersBehavior)step.Data.Behaviors.Data.Behaviors.FirstOrDefault(behavior => behavior is ExecuteChaptersBehavior);
                }

                return behavior;
            }
        }

        public ParallelExecutionNode(IStep step) : base(step)
        {
            titleButtonContainer.Clear();
            extensionContainer.style.backgroundColor = new Color(.2f, .2f, .2f, .8f);
            DrawButtons();
            RefreshExpandedState();
            //RegisterCallback<MouseDownEvent>(e => OnNodeClicked(e));
        }

        //private void OnNodeClicked(MouseDownEvent e)
        //{
        //    if ((e.clickCount == 2) && e.button == (int)MouseButton.LeftMouse && IsRenamable())
        //    {
        //        ExpandNode();
        //        e.PreventDefault();
        //        e.StopImmediatePropagation();
        //    }
        //}

        private void DrawButtons()
        {
            foreach (IChapter chapter in Behavior.Data.Chapters)
            {
                Button expandButton = new Button(() => ExpandNode(chapter));
                expandButton.text = chapter.Data.Name;
                extensionContainer.Add(expandButton);
            }

            Button addPathButton = new Button(() => AddNewPath());
            addPathButton.text = "+";
            extensionContainer.Add(addPathButton);
        }

        private void AddNewPath()
        {
            IChapter path = EntityFactory.CreateChapter($"Path {Behavior.Data.Chapters.Count + 1}");
            Behavior.Data.Chapters.Add(path);
            Button expandButton = new Button(() => ExpandNode(path));
            expandButton.text = path.Data.Name;
            extensionContainer.Insert(extensionContainer.childCount - 1, expandButton);
        }

        private void ExplodeNode()
        {
            //IChapter currentChapter = GlobalEditorHandler.GetCurrentChapter();
            //IEnumerable<ITransition> leadingTransitions = new List<ITransition>(currentChapter.Data.Steps.SelectMany(step => step.Data.Transitions.Data.Transitions).Where(transition => transition.Data.TargetStep == step));
            //List<Vector2> originalPositions = new List<Vector2>(Behavior.Data.Chapter.Data.Steps.Select(step => step.StepMetadata.Position));

            //RevertableChangesHandler.Do(new ProcessCommand(
            //    () =>
            //    {
            //        ExplodeGroup(currentChapter, leadingTransitions);
            //        GlobalEditorHandler.RequestNewChapter(currentChapter);
            //    },
            //    () =>
            //    {
            //        UndoExplodeGroup(currentChapter, leadingTransitions, originalPositions);
            //        GlobalEditorHandler.RequestNewChapter(currentChapter);
            //    }
            //));
        }

        private void ExplodeGroup(IChapter currentChapter, IEnumerable<ITransition> leadingTransitions)
        {
            //foreach (IStep addedStep in Behavior.Data.Chapter.Data.Steps)
            //{
            //    currentChapter.Data.Steps.Add(addedStep);
            //    Vector2 newPosition = addedStep.StepMetadata.Position - behavior.Data.Chapter.Data.Steps.Select(step => step.StepMetadata.Position).OrderBy(position => (position - Behavior.Data.Chapter.ChapterMetadata.EntryNodePosition).sqrMagnitude).First();
            //    addedStep.StepMetadata.Position = step.StepMetadata.Position + newPosition * explodeScaleFactor;
            //}

            //foreach (ITransition transition in leadingTransitions)
            //{
            //    transition.Data.TargetStep = Behavior.Data.Chapter.Data.FirstStep;
            //}

            //if (currentChapter.Data.FirstStep == step)
            //{
            //    currentChapter.Data.FirstStep = Behavior.Data.Chapter.Data.FirstStep;
            //}

            //currentChapter.Data.Steps.Remove(step);
        }

        private void UndoExplodeGroup(IChapter currentChapter, IEnumerable<ITransition> leadingTransitions, List<Vector2> originalPositions)
        {
            //currentChapter.Data.Steps.Add(step);

            //foreach (ITransition transition in leadingTransitions)
            //{
            //    transition.Data.TargetStep = step;
            //}

            //if(Behavior.Data.Chapter.Data.Steps.Contains(currentChapter.Data.FirstStep))
            //{
            //    currentChapter.Data.FirstStep = step;
            //}

            //for(int i = 0; i < Behavior.Data.Chapter.Data.Steps.Count(); i++)
            //{
            //    IStep step = Behavior.Data.Chapter.Data.Steps[i];
            //    step.StepMetadata.Position = originalPositions[i];
            //    currentChapter.Data.Steps.Remove(step);
            //}
        }

        public void GroupSteps(IChapter currentChapter, IEnumerable<IStep> steps)
        {
            IEnumerable<ITransition> leadingTransitions = currentChapter.Data.Steps.SelectMany(step => step.Data.Transitions.Data.Transitions).Where(transition => steps.Contains(transition.Data.TargetStep));
        }

        private void ExpandNode(IChapter chapter)
        {
            IChapter currentChapter = GlobalEditorHandler.GetCurrentChapter();

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    GlobalEditorHandler.RequestNewChapter(chapter);
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
    }
}
