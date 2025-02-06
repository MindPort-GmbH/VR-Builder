using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.UndoRedo;
using VRBuilder.Core.Entities.Factories;

namespace VRBuilder.Core.Editor.UI.GraphView.Nodes
{
    /// <summary>
    /// Graphical representation of a Parallel Execution node.
    /// </summary>
    public class ParallelExecutionNode : StepGraphNode
    {
        public static string DefaultThreadName = "Parallel Path";
        private static string optionalIconFileName = "icon-optional";
        private static string nonOptionalIconFileName = "icon-non-optional";

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
            foreach (SubChapter subChapter in Behavior.Data.SubChapters)
            {
                ThreadElement threadElement = new ThreadElement(subChapter, GetIcon(editIconFileName), GetIcon(deleteIconFileName), GetIcon(optionalIconFileName), GetIcon(nonOptionalIconFileName), () => ViewThread(subChapter.Chapter), () => DeleteThread(subChapter)); //TODO
                extensionContainer.Add(threadElement);
            }

            Button addPathButton = new Button(() => AddNewThread());
            addPathButton.text = "+";
            extensionContainer.Add(addPathButton);
        }

        private void DeleteThread(SubChapter subChapter)
        {
            int subChapterIndex = Behavior.Data.SubChapters.IndexOf(subChapter);

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    Behavior.Data.SubChapters.Remove(subChapter);
                    extensionContainer.RemoveAt(subChapterIndex);
                },
                () =>
                {
                    Behavior.Data.SubChapters.Insert(subChapterIndex, subChapter);
                    extensionContainer.Insert(subChapterIndex, new ThreadElement(subChapter, GetIcon(editIconFileName), GetIcon(deleteIconFileName), GetIcon(optionalIconFileName), GetIcon(nonOptionalIconFileName), () => ViewThread(subChapter.Chapter), () => DeleteThread(subChapter))); //TODO
                }
            ));
        }

        private void AddNewThread()
        {
            SubChapter subChapter = new SubChapter(EntityFactory.CreateChapter($"{DefaultThreadName} {Behavior.Data.SubChapters.Count + 1}"));

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    Behavior.Data.SubChapters.Add(subChapter);
                    ThreadElement threadElement = new ThreadElement(subChapter, GetIcon(editIconFileName), GetIcon(deleteIconFileName), GetIcon(optionalIconFileName), GetIcon(nonOptionalIconFileName), () => ViewThread(subChapter.Chapter), () => DeleteThread(subChapter)); //TODO
                    extensionContainer.Insert(extensionContainer.childCount - 1, threadElement);
                },
                () =>
                {
                    int index = behavior.Data.SubChapters.IndexOf(subChapter);
                    behavior.Data.SubChapters.Remove(subChapter);
                    extensionContainer.RemoveAt(index);
                }
            ));
        }

        /// <summary>
        /// Graphical representation of a single thread in a Parallel Execution node.
        /// </summary>
        private class ThreadElement : VisualElement
        {
            private SubChapter subChapter;
            private Image editIcon;
            private Image deleteIcon;
            private Image optionalIcon;
            private Image nonOptionalIcon;
            private Action onClick;
            private Action onDelete;
            private TextField textField;

            private Button expandButton;
            private Button optionalButton;
            private Button renameButton;
            private Button deleteButton;

            public ThreadElement(SubChapter subChapter, Image editIcon, Image deleteIcon, Image optionalIcon, Image nonOptionalIcon, Action onClick, Action onDelete)
            {
                this.subChapter = subChapter;
                this.editIcon = editIcon;
                this.deleteIcon = deleteIcon;
                this.onClick = onClick;
                this.onDelete = onDelete;
                this.optionalIcon = optionalIcon;
                this.nonOptionalIcon = nonOptionalIcon;

                contentContainer.style.flexDirection = FlexDirection.Row;
                contentContainer.style.justifyContent = Justify.SpaceBetween;

                DrawButtons();
            }

            private void DrawButtons()
            {
                contentContainer.Clear();

                expandButton = new Button(onClick);
                expandButton.text = subChapter.Chapter.Data.Name;
                expandButton.style.flexGrow = 1;
                contentContainer.Add(expandButton);

                optionalButton = new Button(OnOptional);
                optionalButton.Add(optionalIcon);
                optionalButton.Add(nonOptionalIcon);
                optionalButton.style.width = 16;
                optionalIcon.StretchToParentSize();
                nonOptionalIcon.StretchToParentSize();
                optionalIcon.visible = subChapter.IsOptional;
                nonOptionalIcon.visible = !subChapter.IsOptional;
                optionalButton.tooltip = subChapter.IsOptional ? "Optional path" : "Non-optional path";
                optionalButton.style.width = 16;
                optionalButton.focusable = false;
                contentContainer.Add(optionalButton);

                renameButton = new Button(() => DrawRenameMode());
                renameButton.Add(editIcon);
                renameButton.style.width = 16;
                renameButton.tooltip = "Rename";
                editIcon.StretchToParentSize();
                contentContainer.Add(renameButton);

                deleteButton = new Button(onDelete);
                deleteButton.Add(deleteIcon);
                deleteButton.style.width = 16;
                deleteButton.tooltip = "Delete";
                deleteIcon.StretchToParentSize();
                contentContainer.Add(deleteButton);
            }

            private void OnOptional()
            {
                subChapter.IsOptional = !subChapter.IsOptional;
                optionalIcon.visible = subChapter.IsOptional;
                nonOptionalIcon.visible = !subChapter.IsOptional;
                optionalButton.tooltip = subChapter.IsOptional ? "Optional path" : "Non-optional path";
            }

            private void DrawRenameMode()
            {
                contentContainer.Clear();

                textField = new TextField();
                textField.style.flexGrow = 1;
                textField.SetValueWithoutNotify(subChapter.Chapter.Data.Name);
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
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    DoneRenaming(textField.text);
                }

                if (evt.keyCode == KeyCode.Escape)
                {
                    DrawButtons();
                }
            }

            private void DoneRenaming(string name)
            {
                string previousName = subChapter.Chapter.Data.Name;
                if (string.IsNullOrEmpty(name) == false)
                {
                    RevertableChangesHandler.Do(new ProcessCommand(
                        () =>
                        {
                            subChapter.Chapter.Data.SetName(name);
                            DrawButtons();
                        },
                        () =>
                        {
                            subChapter.Chapter.Data.SetName(previousName);
                            DrawButtons();
                        }
                    ));
                }
            }
        }
    }
}
