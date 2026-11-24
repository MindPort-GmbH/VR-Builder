// Copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.UI.NewStepInspector.Drawers;
using VRBuilder.Core.Editor.UI.NewStepInspector.Elements;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;
using VRBuilder.Core.Editor.UndoRedo;
using VRBuilder.Core.Editor.Utils;
using DisplayNameAttribute = VRBuilder.Core.Attributes.DisplayNameAttribute;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Tabs
{
    /// <summary>
    /// Behaviors tab: vertical scroll view with foldable behavior sections,
    /// matching the IMGUI Step Inspector layout.
    /// </summary>
    public class BehaviorsTab : IStepInspectorTab
    {
        public string TabLabel => "Behaviors";
        public bool HasWarning { get; private set; }

        private IStep step;
        private IChapter chapter;
        private IProcess process;

        private ScrollView scrollView;

        /// <summary>
        /// Tracks fold state per behavior object identity (not index).
        /// This survives reordering and deletion of other behaviors.
        /// </summary>
        private readonly Dictionary<IBehavior, bool> foldStates = new Dictionary<IBehavior, bool>();

        public VisualElement BuildContent()
        {
            scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            return scrollView;
        }

        public void Refresh(IStep step, IChapter chapter, IProcess process)
        {
            this.step = step;
            this.chapter = chapter;
            this.process = process;

            // Don't clear foldStates on step change — they are keyed by IBehavior
            // object identity, so each behavior retains its own fold state even
            // when switching between steps and coming back.

            RebuildContent();
            UpdateHasWarning();
        }

        public void Dispose() { }

        private void RebuildContent()
        {
            scrollView.Clear();

            IList<IBehavior> behaviors = GetBehaviors();
            if (behaviors == null || behaviors.Count == 0)
            {
                // Still show the add button even when no behaviors exist
                scrollView.Add(CreateBottomButtons());
                return;
            }

            for (int i = 0; i < behaviors.Count; i++)
            {
                int index = i;
                IBehavior behavior = behaviors[index];
                if (behavior == null) continue;

                // Separator before each item (except first)
                if (index > 0)
                {
                    scrollView.Add(FoldableItemElement.CreateSeparator());
                }

                // Get display name
                string displayName = GetBehaviorDisplayName(behavior);

                // Get fold state keyed by behavior identity (default: folded)
                if (!foldStates.ContainsKey(behavior))
                {
                    foldStates[behavior] = true;
                }

                bool isFirst = index == 0;
                bool isLast = index == behaviors.Count - 1;

                FoldableItemElement foldableItem = new FoldableItemElement(
                    displayName,
                    startFolded: foldStates[behavior],
                    isFirst: isFirst,
                    isLast: isLast);

                // Wire up callbacks
                int capturedIndex = index;
                IBehavior capturedBehavior = behavior;

                foldableItem.OnDelete = () => RemoveBehavior(capturedIndex);
                foldableItem.OnRemove = () => RemoveBehavior(capturedIndex);
                foldableItem.OnMoveUp = () => MoveBehavior(capturedIndex, -1);
                foldableItem.OnMoveDown = () => MoveBehavior(capturedIndex, 1);
                foldableItem.OnCopy = () => SystemClipboard.CopyEntity(capturedBehavior);
                foldableItem.OnPaste = SystemClipboard.IsEntityInClipboard<IBehavior>()
                    ? (Action)(() => PasteBehavior(capturedIndex))
                    : null;

                // Track fold state changes via the explicit callback (not ChangeEvent<bool>)
                foldableItem.OnFoldStateChanged = (bool newFolded) =>
                {
                    foldStates[capturedBehavior] = newFolded;
                };

                // Build the inner content using the drawer system
                BuildBehaviorContent(foldableItem.Content, behavior);

                // Add "Wait for completion" toggle if applicable
                if (behavior.Data is IBackgroundBehaviorData backgroundData)
                {
                    Toggle blockingToggle = new Toggle("Wait for completion");
                    blockingToggle.AddToClassList("step-inspector__blocking-toggle");
                    blockingToggle.SetValueWithoutNotify(backgroundData.IsBlocking);

                    blockingToggle.RegisterValueChangedCallback(evt =>
                    {
                        // Stop propagation to prevent bubbling to parent elements
                        evt.StopPropagation();

                        bool oldValue = backgroundData.IsBlocking;
                        bool newValue = evt.newValue;

                        RevertableChangesHandler.Do(new ProcessCommand(
                            () =>
                            {
                                backgroundData.IsBlocking = newValue;
                                GlobalEditorHandler.CurrentProcessModified();
                            },
                            () =>
                            {
                                backgroundData.IsBlocking = oldValue;
                                GlobalEditorHandler.CurrentProcessModified();
                            }));
                    });

                    foldableItem.Content.Add(blockingToggle);
                }

                scrollView.Add(foldableItem);
            }

            // Separator before buttons
            scrollView.Add(FoldableItemElement.CreateSeparator());

            // Add/Paste buttons at bottom
            scrollView.Add(CreateBottomButtons());
        }

        private void BuildBehaviorContent(VisualElement container, IBehavior behavior)
        {
            IElementDrawer drawer = ElementDrawerLocator.GetDrawerForValue(behavior, behavior.GetType());
            if (drawer == null) return;

            string label = drawer.GetLabel(behavior, behavior.GetType());
            VisualElement element = drawer.CreateElement(behavior, (newValue) =>
            {
                GlobalEditorHandler.CurrentProcessModified();
                // Rebuild to reflect updated values. Fold states are keyed by
                // behavior identity so they survive the rebuild.
                RebuildContent();
            }, label);

            if (element != null)
            {
                container.Add(element);
            }
        }

        private VisualElement CreateBottomButtons()
        {
            VisualElement buttonRow = new VisualElement();
            buttonRow.AddToClassList("step-inspector__button-row");

            // Add Behavior button with dropdown menu
            Button addButton = FoldableItemElement.CreateAddButton("Add Behavior", ShowAddBehaviorMenu);

            // Paste button
            Button pasteButton = FoldableItemElement.CreatePasteButton(() =>
            {
                PasteBehaviorAtEnd();
            });
            pasteButton.SetEnabled(SystemClipboard.IsEntityInClipboard<IBehavior>());

            buttonRow.Add(addButton);
            buttonRow.Add(pasteButton);
            return buttonRow;
        }

        private void ShowAddBehaviorMenu()
        {
            try
            {
                GenericMenu menu = new GenericMenu();
                var menuContent = EditorConfigurator.Instance.BehaviorsMenuContent;
                if (menuContent != null)
                {
                    foreach (MenuOption<IBehavior> option in menuContent)
                    {
                        if (option is MenuItem<IBehavior> menuItem)
                        {
                            string displayedName = menuItem.DisplayedName;
                            menu.AddItem(new GUIContent(displayedName), false, () =>
                            {
                                IBehavior newBehavior = menuItem.GetNewItem();
                                AddBehavior(newBehavior);
                            });
                        }
                        else if (option is MenuSeparator<IBehavior> separator)
                        {
                            menu.AddSeparator(separator.PathToSubmenu ?? "");
                        }
                    }
                }
                menu.ShowAsContext();
            }
            catch (Exception)
            {
                // EditorConfigurator may not be available
            }
        }

        private void AddBehavior(IBehavior behavior)
        {
            if (step == null || behavior == null) return;
            IList<IBehavior> list = GetBehaviors();
            if (list == null) return;

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    list.Add(behavior);
                    GlobalEditorHandler.CurrentProcessModified();
                    RebuildContent();
                },
                () =>
                {
                    list.Remove(behavior);
                    foldStates.Remove(behavior);
                    GlobalEditorHandler.CurrentProcessModified();
                    RebuildContent();
                }));
        }

        private void RemoveBehavior(int index)
        {
            IList<IBehavior> list = GetBehaviors();
            if (index < 0 || index >= list.Count) return;

            IBehavior removed = list[index];
            bool removedFoldState = foldStates.ContainsKey(removed) ? foldStates[removed] : true;

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    list.RemoveAt(index);
                    foldStates.Remove(removed);
                    GlobalEditorHandler.CurrentProcessModified();
                    RebuildContent();
                },
                () =>
                {
                    list.Insert(index, removed);
                    foldStates[removed] = removedFoldState;
                    GlobalEditorHandler.CurrentProcessModified();
                    RebuildContent();
                }));
        }

        private void MoveBehavior(int fromIndex, int direction)
        {
            IList<IBehavior> list = GetBehaviors();
            int toIndex = fromIndex + direction;

            if (fromIndex < 0 || fromIndex >= list.Count) return;
            if (toIndex < 0 || toIndex >= list.Count) return;

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    IBehavior temp = list[fromIndex];
                    list[fromIndex] = list[toIndex];
                    list[toIndex] = temp;
                    GlobalEditorHandler.CurrentProcessModified();
                    RebuildContent();
                },
                () =>
                {
                    IBehavior temp = list[fromIndex];
                    list[fromIndex] = list[toIndex];
                    list[toIndex] = temp;
                    GlobalEditorHandler.CurrentProcessModified();
                    RebuildContent();
                }));
        }

        private void PasteBehavior(int afterIndex)
        {
            try
            {
                IEntity entity = SystemClipboard.PasteEntity();
                if (entity is IBehavior behavior)
                {
                    IList<IBehavior> list = GetBehaviors();
                    int insertIndex = afterIndex + 1;

                    RevertableChangesHandler.Do(new ProcessCommand(
                        () =>
                        {
                            list.Insert(insertIndex, behavior);
                            GlobalEditorHandler.CurrentProcessModified();
                            RebuildContent();
                        },
                        () =>
                        {
                            list.Remove(behavior);
                            foldStates.Remove(behavior);
                            GlobalEditorHandler.CurrentProcessModified();
                            RebuildContent();
                        }));
                }
            }
            catch (Exception)
            {
                // Paste failed - clipboard doesn't contain valid behavior data
            }
        }

        private void PasteBehaviorAtEnd()
        {
            try
            {
                IEntity entity = SystemClipboard.PasteEntity();
                if (entity is IBehavior behavior)
                {
                    AddBehavior(behavior);
                }
            }
            catch (Exception)
            {
                // Paste failed
            }
        }

        private void UpdateHasWarning()
        {
            HasWarning = false;
            try
            {
                if (step?.Data != null
                    && EditorConfigurator.Instance.Validation?.LastReport != null)
                {
                    HasWarning = EditorConfigurator.Instance.Validation.LastReport
                        .GetBehaviorEntriesFor(step.Data).Count > 0;
                }
            }
            catch (Exception)
            {
                // Validation may not be available
            }
        }

        private string GetBehaviorDisplayName(IBehavior behavior)
        {
            string name = behavior?.Data?.Name;
            if (!string.IsNullOrEmpty(name)) return name;

            // Try to get display name from attribute
            Type behaviorType = behavior?.GetType();
            if (behaviorType != null)
            {
                var displayNameAttr = behaviorType
                    .GetCustomAttributes(typeof(DisplayNameAttribute), true)
                    .FirstOrDefault() as DisplayNameAttribute;

                if (displayNameAttr != null && !string.IsNullOrEmpty(displayNameAttr.Name))
                {
                    return displayNameAttr.Name;
                }

                return behaviorType.Name;
            }

            return "Behavior";
        }

        private IList<IBehavior> GetBehaviors()
        {
            return step?.Data?.Behaviors?.Data?.Behaviors ?? (IList<IBehavior>)Array.Empty<IBehavior>();
        }
    }
}
