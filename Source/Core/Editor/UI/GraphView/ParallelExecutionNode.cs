using System;
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
        private EditorIcon editIcon;
        public static string DefaultThreadName = "Thread";

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

        protected Image GetEditIcon()
        {
            if (editIcon == null)
            {
                editIcon = new EditorIcon("icon_edit");
            }

            Image icon = new Image();
            icon.image = editIcon.Texture;
            icon.style.paddingBottom = 2;
            icon.style.paddingLeft = 2;
            icon.style.paddingRight = 2;
            icon.style.paddingTop = 2;

            return icon;
        }

        public override void Refresh()
        {
            base.Refresh();

            extensionContainer.Clear();
            DrawButtons();
        }


        private void DrawButtons()
        {
            foreach (IChapter chapter in Behavior.Data.Chapters)
            {
                //Button expandButton = new Button(() => ExpandNode(chapter));
                //expandButton.text = chapter.Data.Name;
                //extensionContainer.Add(expandButton);
                ThreadElement threadElement = new ThreadElement(chapter, GetEditIcon(), CreateDeleteTransitionIcon(), () => ExpandNode(chapter), () => RenameThread(chapter), () => DeleteThread(chapter));
                extensionContainer.Add(threadElement);
            }

            Button addPathButton = new Button(() => AddNewThread());
            addPathButton.text = "+";
            extensionContainer.Add(addPathButton);
        }

        private void DeleteThread(IChapter chapter)
        {
            Behavior.Data.Chapters.Remove(chapter);
            Refresh();
        }

        private void RenameThread(IChapter chapter)
        {
            throw new NotImplementedException();
        }

        private void AddNewThread()
        {
            IChapter thread = EntityFactory.CreateChapter($"{DefaultThreadName} {Behavior.Data.Chapters.Count + 1}");
            Behavior.Data.Chapters.Add(thread);
            ThreadElement threadElement = new ThreadElement(thread, GetEditIcon(), CreateDeleteTransitionIcon(), () => ExpandNode(thread), () => RenameThread(thread), () => DeleteThread(thread));
            extensionContainer.Insert(extensionContainer.childCount - 1, threadElement);
        }

        private class ThreadElement : VisualElement
        {
            public ThreadElement(IChapter chapter, Image editIcon, Image deleteIcon, Action onClick, Action onEdit, Action onDelete)
            {
                contentContainer.style.flexDirection = FlexDirection.Row;
                //contentContainer.style.justifyContent = Justify.SpaceBetween;

                Button expandButton = new Button(onClick);
                expandButton.text = chapter.Data.Name;
                expandButton.style.flexGrow = 1;
                contentContainer.Add(expandButton);

                Button renameButton = new Button(onEdit);
                renameButton.Add(editIcon);
                renameButton.style.width = 16;
                editIcon.StretchToParentSize();
                contentContainer.Add(renameButton);

                Button deleteButton = new Button(onDelete);
                deleteButton.Add(deleteIcon);
                deleteButton.style.width = 16;
                deleteIcon.StretchToParentSize();
                contentContainer.Add(deleteButton);
            }
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
