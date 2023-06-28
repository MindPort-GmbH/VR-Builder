using System;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;
using VRBuilder.Editor.UndoRedo;

namespace VRBuilder.Editor.UI.Graphics
{
    /// <summary>
    /// Graphical representation of a Parallel Execution node.
    /// </summary>
    public class ParallelExecutionNode : StepGraphNode
    {
        public static string DefaultThreadName = "Parallel Path";

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
            DrawButtons();
            RefreshExpandedState();
        }

        public override void Refresh()
        {
            base.Refresh();

            extensionContainer.Clear();
            DrawButtons();
            RefreshExpandedState();
        }

        public override void OnSelected()
        {
            base.OnSelected();

            GlobalEditorHandler.ChangeCurrentStep(null);
        }

        private void ViewThread(IChapter chapter)
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

        private void DrawButtons()
        {
            foreach (IChapter chapter in Behavior.Data.Chapters)
            {
                ThreadElement threadElement = new ThreadElement(chapter, GetIcon(editIconFileName), GetIcon(deleteIconFileName), () => ViewThread(chapter), () => DeleteThread(chapter));
                extensionContainer.Add(threadElement);
            }

            Button addPathButton = new Button(() => AddNewThread());
            addPathButton.text = "+";
            extensionContainer.Add(addPathButton);
        }

        private void DeleteThread(IChapter chapter)
        {
            int chapterIndex = Behavior.Data.Chapters.IndexOf(chapter);

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    Behavior.Data.Chapters.Remove(chapter);
                    extensionContainer.RemoveAt(chapterIndex);
                },
                () =>
                {
                    Behavior.Data.Chapters.Insert(chapterIndex, chapter);
                    extensionContainer.Insert(chapterIndex, new ThreadElement(chapter, GetIcon(editIconFileName), GetIcon(deleteIconFileName), () => ViewThread(chapter), () => DeleteThread(chapter)));
                }
            ));
        }

        private void AddNewThread()
        {
            IChapter thread = EntityFactory.CreateChapter($"{DefaultThreadName} {Behavior.Data.Chapters.Count + 1}");

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    Behavior.Data.Chapters.Add(thread);
                    ThreadElement threadElement = new ThreadElement(thread, GetIcon(editIconFileName), GetIcon(deleteIconFileName), () => ViewThread(thread), () => DeleteThread(thread));
                    extensionContainer.Insert(extensionContainer.childCount - 1, threadElement);
                },
                () =>
                {
                    int index = behavior.Data.Chapters.IndexOf(thread);
                    behavior.Data.Chapters.Remove(thread);
                    extensionContainer.RemoveAt(index);
                }
            ));
        }

        /// <summary>
        /// Graphical representation of a single thread in a Parallel Execution node.
        /// </summary>
        private class ThreadElement : VisualElement
        {
            private IChapter chapter;
            private Image editIcon;
            private Image deleteIcon;
            private Action onClick;
            private Action onDelete;
            private TextField textField;

            public ThreadElement(IChapter chapter, Image editIcon, Image deleteIcon, Action onClick, Action onDelete)
            {
                this.chapter = chapter;
                this.editIcon = editIcon;
                this.deleteIcon = deleteIcon;
                this.onClick = onClick;
                this.onDelete = onDelete;

                contentContainer.style.flexDirection = FlexDirection.Row;
                contentContainer.style.justifyContent = Justify.SpaceBetween;

                DrawButtons();
            }

            private void DrawButtons()
            {
                contentContainer.Clear();

                Button expandButton = new Button(onClick);
                expandButton.text = chapter.Data.Name;
                expandButton.style.flexGrow = 1;
                contentContainer.Add(expandButton);

                Button renameButton = new Button(() => DrawRenameMode());
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

            private void DrawRenameMode()
            {
                contentContainer.Clear();

                textField = new TextField();
                textField.style.flexGrow = 1;
                textField.SetValueWithoutNotify(chapter.Data.Name);
                contentContainer.Add(textField);
                textField.Focus();
                textField.SelectAll();
                textField.RegisterCallback<KeyDownEvent>(OnKeyDown);

                Button doneButton = new Button(() => DoneRenaming(textField.text));
                doneButton.text = "Done";
                contentContainer.Add(doneButton);                
            }

            private void OnKeyDown(KeyDownEvent evt)
            {
                if(evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    DoneRenaming(textField.text);
                }

                if(evt.keyCode == KeyCode.Escape)
                {
                    DrawButtons();
                }
            }

            private void DoneRenaming(string name)
            {
                string previousName = chapter.Data.Name;
                if (string.IsNullOrEmpty(name) == false)
                {
                    RevertableChangesHandler.Do(new ProcessCommand(
                        () =>
                        {
                            chapter.Data.SetName(name);
                            DrawButtons();
                        },
                        () =>
                        {
                            chapter.Data.SetName(previousName);
                            DrawButtons();
                        }
                    ));

                }
            }            
        }
    }
}
