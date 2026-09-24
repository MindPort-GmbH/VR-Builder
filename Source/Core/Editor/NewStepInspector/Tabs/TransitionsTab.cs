// Copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.UI.Drawers;
using VRBuilder.Core.Editor.UI.NewStepInspector.Drawers;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;
using VRBuilder.Core.Editor.UndoRedo;
using VRBuilder.Core.Entities.Factories;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Tabs
{
    /// <summary>
    /// Transitions tab: TwoPaneSplitView with transition list (left) and
    /// target step popup + condition list + condition detail (right).
    /// </summary>
    public class TransitionsTab : IStepInspectorTab
    {
        public string TabLabel => "Transitions";
        public bool HasWarning { get; private set; }

        private IStep step;
        private IChapter chapter;
        private IProcess process;

        private ListView transitionListView;
        private PopupField<string> transitionTargetPopup;
        private ListView conditionListView;
        private ScrollView conditionDetailScrollView;
        private Button removeTransitionButton;
        private Button removeConditionButton;
        private Button moveConditionUpButton;
        private Button moveConditionDownButton;

        private int selectedTransitionIndex = -1;
        private int selectedConditionIndex = -1;
        private bool suppressCallbacks;

        private readonly List<IStep> targetChoices = new List<IStep>();

        public VisualElement BuildContent()
        {
            TwoPaneSplitView split = new TwoPaneSplitView(0, 280, TwoPaneSplitViewOrientation.Horizontal);
            split.AddToClassList("step-inspector__split");

            // Left pane: transition list
            VisualElement left = new VisualElement();
            left.AddToClassList("step-inspector__list-pane");

            Toolbar transitionToolbar = new Toolbar();
            Button addTransitionButton = new Button(AddTransition) { text = "Add Transition" };
            removeTransitionButton = new Button(RemoveSelectedTransition) { text = "Remove" };

            transitionToolbar.Add(addTransitionButton);
            transitionToolbar.Add(removeTransitionButton);
            left.Add(transitionToolbar);

            transitionListView = new ListView
            {
                selectionType = SelectionType.Single,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showBorder = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                makeItem = () =>
                {
                    Label label = new Label();
                    label.AddToClassList("step-inspector__list-item");
                    return label;
                },
                bindItem = (element, index) =>
                {
                    Label label = (Label)element;
                    IList<ITransition> list = GetTransitions();
                    if (index >= 0 && index < list.Count)
                    {
                        ITransition transition = list[index];
                        string targetName = transition?.Data?.TargetStep != null
                            ? (transition.Data.TargetStep.Data?.Name ?? "Step")
                            : "End Chapter";
                        label.text = $"Transition {index} \u2192 {targetName}";
                    }
                    else
                    {
                        label.text = $"Transition {index}";
                    }
                }
            };
            transitionListView.AddToClassList("step-inspector__list-view");
            transitionListView.selectionChanged += _ =>
            {
                if (suppressCallbacks) return;
                selectedTransitionIndex = transitionListView.selectedIndex;
                selectedConditionIndex = -1;
                RefreshTransitionDetail();
            };
            left.Add(transitionListView);

            // Right pane: transition detail
            VisualElement right = new VisualElement();
            right.AddToClassList("step-inspector__detail-pane");

            Label detailTitle = new Label("Transition Details");
            detailTitle.AddToClassList("step-inspector__section-title");
            right.Add(detailTitle);

            transitionTargetPopup = new PopupField<string>("Target Step", new List<string> { "<End Chapter>" }, 0);
            transitionTargetPopup.RegisterValueChangedCallback(evt =>
            {
                if (suppressCallbacks) return;
                ApplyTransitionTarget(evt.newValue);
            });
            right.Add(transitionTargetPopup);

            // Condition toolbar
            Toolbar conditionToolbar = new Toolbar();
            ToolbarMenu addConditionMenu = new ToolbarMenu { text = "Add Condition" };
            PopulateConditionMenu(addConditionMenu);

            removeConditionButton = new Button(RemoveSelectedCondition) { text = "Remove" };
            moveConditionUpButton = new Button(() => MoveCondition(-1)) { text = "Up" };
            moveConditionDownButton = new Button(() => MoveCondition(1)) { text = "Down" };

            conditionToolbar.Add(addConditionMenu);
            conditionToolbar.Add(removeConditionButton);
            conditionToolbar.Add(moveConditionUpButton);
            conditionToolbar.Add(moveConditionDownButton);
            right.Add(conditionToolbar);

            conditionListView = new ListView
            {
                selectionType = SelectionType.Single,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showBorder = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                makeItem = () =>
                {
                    Label label = new Label();
                    label.AddToClassList("step-inspector__list-item");
                    return label;
                },
                bindItem = (element, index) =>
                {
                    Label label = (Label)element;
                    IList<ICondition> list = GetConditions();
                    if (index >= 0 && index < list.Count)
                    {
                        ICondition condition = list[index];
                        string displayName = condition?.Data?.Name;
                        if (string.IsNullOrEmpty(displayName))
                        {
                            displayName = condition?.GetType().Name ?? $"Condition {index}";
                        }
                        label.text = displayName;
                    }
                    else
                    {
                        label.text = $"Condition {index}";
                    }
                }
            };
            conditionListView.AddToClassList("step-inspector__list-view--fixed-height");
            conditionListView.selectionChanged += _ =>
            {
                if (suppressCallbacks) return;
                selectedConditionIndex = conditionListView.selectedIndex;
                RebuildConditionDetails();
                RefreshConditionButtons();
            };
            right.Add(conditionListView);

            Label conditionDetailTitle = new Label("Condition Details");
            conditionDetailTitle.AddToClassList("step-inspector__section-title--with-top-margin");
            right.Add(conditionDetailTitle);

            conditionDetailScrollView = new ScrollView(ScrollViewMode.Vertical);
            conditionDetailScrollView.AddToClassList("step-inspector__detail-scroll");
            right.Add(conditionDetailScrollView);

            split.Add(left);
            split.Add(right);
            return split;
        }

        public void Refresh(IStep step, IChapter chapter, IProcess process)
        {
            this.step = step;
            this.chapter = chapter;
            this.process = process;

            RefreshTransitionList();
            RefreshTransitionDetail();
        }

        public void Dispose() { }

        private void PopulateConditionMenu(ToolbarMenu menu)
        {
            try
            {
                var menuContent = EditorConfigurator.Instance.ConditionsMenuContent;
                if (menuContent != null)
                {
                    foreach (MenuOption<ICondition> option in menuContent)
                    {
                        if (option is MenuItem<ICondition> menuItem)
                        {
                            string displayedName = menuItem.DisplayedName;
                            menu.menu.AppendAction(displayedName, _ =>
                            {
                                ICondition newCondition = menuItem.GetNewItem();
                                AddCondition(newCondition);
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
                // EditorConfigurator may not be available
            }
        }

        private void AddTransition()
        {
            if (step == null) return;
            IList<ITransition> list = GetTransitions();
            ITransition transition = EntityFactory.CreateTransition();

            ExecuteMutation(
                () =>
                {
                    list.Add(transition);
                    selectedTransitionIndex = list.Count - 1;
                    selectedConditionIndex = -1;
                },
                () =>
                {
                    list.Remove(transition);
                    selectedTransitionIndex = ClampIndex(selectedTransitionIndex, list.Count);
                    selectedConditionIndex = -1;
                });
        }

        private void RemoveSelectedTransition()
        {
            IList<ITransition> list = GetTransitions();
            if (!IsValidIndex(selectedTransitionIndex, list.Count)) return;

            ITransition removed = list[selectedTransitionIndex];
            int index = selectedTransitionIndex;

            ExecuteMutation(
                () =>
                {
                    list.RemoveAt(index);
                    selectedTransitionIndex = ClampIndex(index, list.Count);
                    selectedConditionIndex = -1;
                },
                () =>
                {
                    list.Insert(index, removed);
                    selectedTransitionIndex = index;
                    selectedConditionIndex = -1;
                });
        }

        private void AddCondition(ICondition condition)
        {
            if (step == null || condition == null) return;
            IList<ICondition> list = GetConditions();
            if (list == null) return;

            ExecuteMutation(
                () =>
                {
                    list.Add(condition);
                    selectedConditionIndex = list.Count - 1;
                },
                () =>
                {
                    list.Remove(condition);
                    selectedConditionIndex = ClampIndex(selectedConditionIndex, list.Count);
                });
        }

        private void RemoveSelectedCondition()
        {
            IList<ICondition> list = GetConditions();
            if (!IsValidIndex(selectedConditionIndex, list.Count)) return;

            ICondition removed = list[selectedConditionIndex];
            int index = selectedConditionIndex;

            ExecuteMutation(
                () =>
                {
                    list.RemoveAt(index);
                    selectedConditionIndex = ClampIndex(index, list.Count);
                },
                () =>
                {
                    list.Insert(index, removed);
                    selectedConditionIndex = index;
                });
        }

        private void MoveCondition(int direction)
        {
            IList<ICondition> list = GetConditions();
            int from = selectedConditionIndex;
            int to = from + direction;

            if (!IsValidIndex(from, list.Count) || !IsValidIndex(to, list.Count)) return;

            ExecuteMutation(
                () =>
                {
                    ICondition temp = list[from];
                    list[from] = list[to];
                    list[to] = temp;
                    selectedConditionIndex = to;
                },
                () =>
                {
                    ICondition temp = list[from];
                    list[from] = list[to];
                    list[to] = temp;
                    selectedConditionIndex = from;
                });
        }

        private void ApplyTransitionTarget(string selectedLabel)
        {
            ITransition transition = GetSelectedTransition();
            if (transition == null) return;

            int choiceIndex = transitionTargetPopup.choices.IndexOf(selectedLabel);
            IStep targetStep = null;

            if (choiceIndex > 0 && choiceIndex - 1 < targetChoices.Count)
            {
                targetStep = targetChoices[choiceIndex - 1];
            }

            IStep oldTarget = transition.Data.TargetStep;
            ExecuteMutation(
                () => transition.Data.TargetStep = targetStep,
                () => transition.Data.TargetStep = oldTarget);
        }

        private void RefreshTransitionList()
        {
            IList<ITransition> list = GetTransitions();

            suppressCallbacks = true;
            transitionListView.itemsSource = list as IList;
            transitionListView.Rebuild();
            selectedTransitionIndex = ClampIndex(selectedTransitionIndex, list.Count);
            transitionListView.selectedIndex = selectedTransitionIndex;
            suppressCallbacks = false;
        }

        private void RefreshTransitionDetail()
        {
            RefreshTargetPopup();
            RefreshConditionList();
            RebuildConditionDetails();
            RefreshConditionButtons();

            removeTransitionButton?.SetEnabled(IsValidIndex(selectedTransitionIndex, GetTransitions().Count));
        }

        private void RefreshTargetPopup()
        {
            ITransition transition = GetSelectedTransition();
            targetChoices.Clear();

            List<string> choices = new List<string> { "<End Chapter>" };

            if (chapter?.Data?.Steps != null)
            {
                foreach (IStep s in chapter.Data.Steps)
                {
                    if (s != null && !ReferenceEquals(s, step))
                    {
                        targetChoices.Add(s);
                        choices.Add(s.Data?.Name ?? "Unnamed Step");
                    }
                }
            }

            suppressCallbacks = true;
            transitionTargetPopup.choices = choices;

            if (transition?.Data?.TargetStep != null)
            {
                int targetIndex = targetChoices.IndexOf(transition.Data.TargetStep);
                transitionTargetPopup.SetValueWithoutNotify(targetIndex >= 0 ? choices[targetIndex + 1] : choices[0]);
            }
            else
            {
                transitionTargetPopup.SetValueWithoutNotify(choices[0]);
            }

            transitionTargetPopup.SetEnabled(transition != null);
            suppressCallbacks = false;
        }

        private void RefreshConditionList()
        {
            IList<ICondition> list = GetConditions();

            suppressCallbacks = true;
            conditionListView.itemsSource = list as IList;
            conditionListView.Rebuild();
            selectedConditionIndex = ClampIndex(selectedConditionIndex, list.Count);
            conditionListView.selectedIndex = selectedConditionIndex;
            suppressCallbacks = false;
        }

        private void RebuildConditionDetails()
        {
            conditionDetailScrollView.Clear();

            IList<ICondition> list = GetConditions();
            if (!IsValidIndex(selectedConditionIndex, list.Count)) return;

            ICondition condition = list[selectedConditionIndex];
            if (condition == null) return;

            IElementDrawer drawer = ElementDrawerLocator.GetDrawerForValue(condition, condition.GetType());
            if (drawer == null) return;

            string label = drawer.GetLabel(condition, condition.GetType());
            VisualElement element = drawer.CreateElement(condition, (newValue) =>
            {
                GlobalEditorHandler.CurrentProcessModified();
                RefreshConditionList();
            }, label);

            if (element != null)
            {
                conditionDetailScrollView.Add(element);
            }
        }

        private void RefreshConditionButtons()
        {
            IList<ICondition> list = GetConditions();
            bool hasSelection = IsValidIndex(selectedConditionIndex, list.Count);

            removeConditionButton?.SetEnabled(hasSelection);
            moveConditionUpButton?.SetEnabled(hasSelection && selectedConditionIndex > 0);
            moveConditionDownButton?.SetEnabled(hasSelection && selectedConditionIndex < list.Count - 1);
        }

        private ITransition GetSelectedTransition()
        {
            IList<ITransition> list = GetTransitions();
            return IsValidIndex(selectedTransitionIndex, list.Count) ? list[selectedTransitionIndex] : null;
        }

        private IList<ITransition> GetTransitions()
        {
            return step?.Data?.Transitions?.Data?.Transitions ?? (IList<ITransition>)Array.Empty<ITransition>();
        }

        private IList<ICondition> GetConditions()
        {
            return GetSelectedTransition()?.Data?.Conditions ?? (IList<ICondition>)Array.Empty<ICondition>();
        }

        private void ExecuteMutation(Action doAction, Action undoAction)
        {
            RevertableChangesHandler.Do(new ProcessCommand(
                () => { doAction(); Refresh(step, chapter, process); },
                () => { undoAction(); Refresh(step, chapter, process); }));
        }

        private static int ClampIndex(int index, int count)
        {
            if (count == 0) return -1;
            return Mathf.Clamp(index, 0, count - 1);
        }

        private static bool IsValidIndex(int index, int count)
        {
            return index >= 0 && index < count;
        }
    }
}
