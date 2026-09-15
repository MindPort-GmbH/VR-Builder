using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Editor;
using VRBuilder.Core.Editor.UndoRedo;
using VRBuilder.Core.Editor.UI.Windows;
using VRBuilder.Core.Entities.Factories;

namespace MindPort.VRBuilderProT.StepInspectorPureUITK
{
    public sealed class StepInspectorPureUITKWindow : EditorWindow
    {
        private enum Tab
        {
            Behaviors,
            Transitions,
        }

        private const string Title = "Step Inspector (Pure UITK Mini)";
        private const string UxmlPath = "Assets/MindPort/VRBuilderProT/Editor/StepInspectorPureUITK/StepInspectorPureUITK.uxml";
        private const string UssPath = "Assets/MindPort/VRBuilderProT/Editor/StepInspectorPureUITK/StepInspectorPureUITK.uss";

        [SerializeField]
        private int activeTabIndex;

        [SerializeField]
        private int selectedBehaviorIndex = -1;

        [SerializeField]
        private int selectedTransitionIndex = -1;

        [SerializeField]
        private int selectedConditionIndex = -1;

        private bool suppressCallbacks;

        private IStep step;
        private IChapter chapter;
        private IProcess process;

        private double lastEditorPollTime;
        private const double EditorPollInterval = 0.1;

        private int lastBehaviorCount = -1;
        private int lastTransitionCount = -1;
        private int lastConditionCount = -1;

        private readonly List<IStep> targetChoices = new List<IStep>();

        private IVisualElementScheduledItem pollTask;

        private HelpBox noStepHelpBox;
        private TextField stepNameField;
        private Label contextLabel;

        private ToolbarToggle behaviorsTabToggle;
        private ToolbarToggle transitionsTabToggle;

        private VisualElement behaviorsPanel;
        private ListView behaviorListView;
        private Button removeBehaviorButton;
        private Button moveBehaviorUpButton;
        private Button moveBehaviorDownButton;
        private VisualElement behaviorDetails;

        private VisualElement transitionsPanel;
        private ListView transitionListView;
        private Button removeTransitionButton;
        private Button moveTransitionUpButton;
        private Button moveTransitionDownButton;
        private PopupField<string> transitionTargetPopup;
        private Button removeConditionButton;
        private Button moveConditionUpButton;
        private Button moveConditionDownButton;
        private ListView conditionListView;
        private VisualElement conditionDetails;

        [MenuItem("Tools/VR Builder/Prototype/Step Inspector (Pure UITK Mini)")]
        public static void OpenWindow()
        {
            StepInspectorPureUITKWindow window = GetWindow<StepInspectorPureUITKWindow>(Title);
            window.minSize = new Vector2(620f, 420f);
            window.Show();
        }

        public static void OpenOrFocus()
        {
            StepInspectorPureUITKWindow window = GetWindow<StepInspectorPureUITKWindow>(Title);
            window.minSize = new Vector2(620f, 420f);
            window.Show();
            window.Focus();
        }

        public static void OpenOrFocusAndBind(IStep selectedStep, IChapter selectedChapter, IProcess selectedProcess, bool focus)
        {
            StepInspectorPureUITKWindow window = GetWindow<StepInspectorPureUITKWindow>(Title, focus);
            window.minSize = new Vector2(620f, 420f);
            window.Show();
            window.SetSelection(selectedStep, selectedChapter, selectedProcess, true);
            if (focus)
            {
                window.Focus();
            }
        }

        public static void BindOpenWindows(IStep selectedStep, IChapter selectedChapter, IProcess selectedProcess)
        {
            foreach (StepInspectorPureUITKWindow window in Resources.FindObjectsOfTypeAll<StepInspectorPureUITKWindow>())
            {
                window.SetSelection(selectedStep, selectedChapter, selectedProcess, true);
            }
        }

        public static void RequestExternalRefresh()
        {
            foreach (StepInspectorPureUITKWindow window in Resources.FindObjectsOfTypeAll<StepInspectorPureUITKWindow>())
            {
                window.TryRefreshAll();
            }
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            EditorApplication.projectChanged += OnExternalDataChanged;
            EditorApplication.hierarchyChanged += OnExternalDataChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            EditorApplication.projectChanged -= OnExternalDataChanged;
            EditorApplication.hierarchyChanged -= OnExternalDataChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= OnEditorUpdate;

            pollTask?.Pause();
            pollTask = null;
        }

        private void OnEditorUpdate()
        {
            double now = EditorApplication.timeSinceStartup;
            if (now - lastEditorPollTime < EditorPollInterval)
            {
                return;
            }

            lastEditorPollTime = now;
            BindCurrentStep(force: false);
        }

        private void CreateGUI()
        {
            rootVisualElement.Clear();

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }

            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (uxml != null)
            {
                uxml.CloneTree(rootVisualElement);
            }

            rootVisualElement.AddToClassList("step-inspector__root");

            QueryHeaderElements();
            QueryContextElements();
            QueryTabElements();
            BuildBehaviorsSplit();
            BuildTransitionsSplit();

            SetActiveTab((Tab)Mathf.Clamp(activeTabIndex, 0, Enum.GetValues(typeof(Tab)).Length - 1));
            BindCurrentStep(force: true);

            pollTask?.Pause();
            pollTask = rootVisualElement.schedule.Execute(PollEditorState).Every(250);
        }

        private void QueryHeaderElements()
        {
            Label titleLabel = rootVisualElement.Q<Label>("header-title");
            titleLabel.text = Title;

            ToolbarButton legacyButton = rootVisualElement.Q<ToolbarButton>("legacy-button");
            legacyButton.clicked += OpenLegacyInspector;
        }

        private void QueryContextElements()
        {
            VisualElement helpBoxContainer = rootVisualElement.Q("no-step-help-box-container");
            noStepHelpBox = new HelpBox("No step selected. Select a step in the Process Editor.", HelpBoxMessageType.Info);
            helpBoxContainer.Add(noStepHelpBox);

            stepNameField = rootVisualElement.Q<TextField>("step-name-field");
            stepNameField.isDelayed = true;
            stepNameField.RegisterValueChangedCallback(evt => ApplyStepName(evt.newValue));

            contextLabel = rootVisualElement.Q<Label>("context-label");
        }

        private void QueryTabElements()
        {
            behaviorsTabToggle = rootVisualElement.Q<ToolbarToggle>("behaviors-tab");
            transitionsTabToggle = rootVisualElement.Q<ToolbarToggle>("transitions-tab");

            behaviorsTabToggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    SetActiveTab(Tab.Behaviors);
                }
            });

            transitionsTabToggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    SetActiveTab(Tab.Transitions);
                }
            });
        }

        private void BuildBehaviorsSplit()
        {
            behaviorsPanel = rootVisualElement.Q("behaviors-panel");

            TwoPaneSplitView split = new TwoPaneSplitView(0, 280, TwoPaneSplitViewOrientation.Horizontal);
            split.AddToClassList("step-inspector__split");

            VisualElement left = new VisualElement();
            left.AddToClassList("step-inspector__list-pane");

            Toolbar leftToolbar = new Toolbar();

            ToolbarMenu addMenu = new ToolbarMenu { text = "Add Behavior" };
            addMenu.menu.AppendAction("Flow/Delay", _ => AddBehavior(new DelayBehavior()));
            addMenu.menu.AppendAction("Guidance/Spawn Confetti", _ => AddBehavior(new ConfettiBehavior()));

            removeBehaviorButton = new Button(RemoveSelectedBehavior) { text = "Remove" };
            moveBehaviorUpButton = new Button(() => MoveBehavior(-1)) { text = "Up" };
            moveBehaviorDownButton = new Button(() => MoveBehavior(1)) { text = "Down" };

            leftToolbar.Add(addMenu);
            leftToolbar.Add(removeBehaviorButton);
            leftToolbar.Add(moveBehaviorUpButton);
            leftToolbar.Add(moveBehaviorDownButton);
            left.Add(leftToolbar);

            behaviorListView = new ListView
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
                    IList<IBehavior> list = GetBehaviors();
                    IBehavior behavior = index >= 0 && index < list.Count ? list[index] : null;
                    label.text = GetBehaviorTitle(behavior, index);
                }
            };
            behaviorListView.AddToClassList("step-inspector__list-view");
            behaviorListView.selectionChanged += _ =>
            {
                if (suppressCallbacks)
                {
                    return;
                }

                selectedBehaviorIndex = behaviorListView.selectedIndex;
                RebuildBehaviorDetails();
                RefreshButtons();
            };
            left.Add(behaviorListView);

            VisualElement right = new VisualElement();
            right.AddToClassList("step-inspector__detail-pane");

            Label detailsTitle = new Label("Behavior Details");
            detailsTitle.AddToClassList("step-inspector__section-title");
            right.Add(detailsTitle);

            behaviorDetails = new ScrollView(ScrollViewMode.Vertical);
            behaviorDetails.AddToClassList("step-inspector__detail-scroll");
            right.Add(behaviorDetails);

            split.Add(left);
            split.Add(right);
            behaviorsPanel.Add(split);
        }

        private void BuildTransitionsSplit()
        {
            transitionsPanel = rootVisualElement.Q("transitions-panel");

            TwoPaneSplitView split = new TwoPaneSplitView(0, 280, TwoPaneSplitViewOrientation.Horizontal);
            split.AddToClassList("step-inspector__split");

            VisualElement left = new VisualElement();
            left.AddToClassList("step-inspector__list-pane");

            Toolbar transitionsToolbar = new Toolbar();

            Button addTransitionButton = new Button(AddTransition) { text = "Add Transition" };
            removeTransitionButton = new Button(RemoveSelectedTransition) { text = "Remove" };
            moveTransitionUpButton = new Button(() => MoveTransition(-1)) { text = "Up" };
            moveTransitionDownButton = new Button(() => MoveTransition(1)) { text = "Down" };

            transitionsToolbar.Add(addTransitionButton);
            transitionsToolbar.Add(removeTransitionButton);
            transitionsToolbar.Add(moveTransitionUpButton);
            transitionsToolbar.Add(moveTransitionDownButton);
            left.Add(transitionsToolbar);

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
                    ITransition transition = index >= 0 && index < list.Count ? list[index] : null;
                    label.text = GetTransitionTitle(transition, index);
                }
            };
            transitionListView.AddToClassList("step-inspector__list-view");
            transitionListView.selectionChanged += _ =>
            {
                if (suppressCallbacks)
                {
                    return;
                }

                selectedTransitionIndex = transitionListView.selectedIndex;
                selectedConditionIndex = -1;
                RefreshConditionList();
                RefreshTransitionTargetPopup();
                RebuildConditionDetails();
                RefreshButtons();
            };
            left.Add(transitionListView);

            VisualElement right = new VisualElement();
            right.AddToClassList("step-inspector__detail-pane");

            Label detailsTitle = new Label("Transition Details");
            detailsTitle.AddToClassList("step-inspector__section-title");
            right.Add(detailsTitle);

            transitionTargetPopup = new PopupField<string>("Target Step", new List<string> { "<End Chapter>" }, 0);
            transitionTargetPopup.RegisterValueChangedCallback(evt => ApplyTransitionTarget(evt.newValue));
            right.Add(transitionTargetPopup);

            Toolbar conditionsToolbar = new Toolbar();
            ToolbarMenu addConditionMenu = new ToolbarMenu { text = "Add Condition" };
            addConditionMenu.menu.AppendAction("Flow/Timeout", _ => AddCondition(new TimeoutCondition()));
            addConditionMenu.menu.AppendAction("Spatial/Object Nearby", _ => AddCondition(new ObjectInRangeCondition()));

            removeConditionButton = new Button(RemoveSelectedCondition) { text = "Remove" };
            moveConditionUpButton = new Button(() => MoveCondition(-1)) { text = "Up" };
            moveConditionDownButton = new Button(() => MoveCondition(1)) { text = "Down" };

            conditionsToolbar.Add(addConditionMenu);
            conditionsToolbar.Add(removeConditionButton);
            conditionsToolbar.Add(moveConditionUpButton);
            conditionsToolbar.Add(moveConditionDownButton);
            right.Add(conditionsToolbar);

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
                    ICondition condition = index >= 0 && index < list.Count ? list[index] : null;
                    label.text = GetConditionTitle(condition, index);
                }
            };
            conditionListView.AddToClassList("step-inspector__list-view--fixed-height");
            conditionListView.selectionChanged += _ =>
            {
                if (suppressCallbacks)
                {
                    return;
                }

                selectedConditionIndex = conditionListView.selectedIndex;
                RebuildConditionDetails();
                RefreshButtons();
            };
            right.Add(conditionListView);

            Label conditionDetailsTitle = new Label("Condition Details");
            conditionDetailsTitle.AddToClassList("step-inspector__section-title--with-top-margin");
            right.Add(conditionDetailsTitle);

            conditionDetails = new ScrollView(ScrollViewMode.Vertical);
            conditionDetails.AddToClassList("step-inspector__detail-scroll");
            right.Add(conditionDetails);

            split.Add(left);
            split.Add(right);
            transitionsPanel.Add(split);
        }

        private void OpenLegacyInspector()
        {
            StepInspectorPureUITKEditingStrategy.AllowLegacyStepWindow = true;
            StepWindow legacyWindow = GetWindow<StepWindow>();
            legacyWindow.Show();
            legacyWindow.Focus();
        }

        private void SetActiveTab(Tab tab)
        {
            activeTabIndex = (int)tab;

            suppressCallbacks = true;
            behaviorsTabToggle.SetValueWithoutNotify(tab == Tab.Behaviors);
            transitionsTabToggle.SetValueWithoutNotify(tab == Tab.Transitions);
            suppressCallbacks = false;

            behaviorsPanel.EnableInClassList("step-inspector__panel--hidden", tab != Tab.Behaviors);
            transitionsPanel.EnableInClassList("step-inspector__panel--hidden", tab != Tab.Transitions);
        }

        private void PollEditorState()
        {
            if (step == null)
            {
                return;
            }

            int behaviorCount = GetBehaviors().Count;
            int transitionCount = GetTransitions().Count;
            int conditionCount = GetConditions().Count;

            if (behaviorCount != lastBehaviorCount || transitionCount != lastTransitionCount || conditionCount != lastConditionCount)
            {
                RefreshAll();
                return;
            }

            if (stepNameField.value != (step.Data.Name ?? string.Empty))
            {
                suppressCallbacks = true;
                stepNameField.SetValueWithoutNotify(step.Data.Name ?? string.Empty);
                suppressCallbacks = false;
            }

            RefreshButtons();
        }

        private void OnUndoRedoPerformed()
        {
            RefreshAll();
        }

        private void OnExternalDataChanged()
        {
            RefreshAll();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange _)
        {
            RefreshAll();
        }

        private void BindCurrentStep(bool force)
        {
            IChapter currentChapter = GlobalEditorHandler.GetCurrentChapter() ?? chapter;
            IStep currentStep = currentChapter?.ChapterMetadata?.LastSelectedStep ?? step;
            IProcess currentProcess = GlobalEditorHandler.GetCurrentProcess() ?? process;

            SetSelection(currentStep, currentChapter, currentProcess, force);
        }

        private void RefreshAll()
        {
            RefreshContext();
            RefreshBehaviorList();
            RefreshTransitionList();
            RefreshConditionList();
            RefreshTransitionTargetPopup();
            RebuildBehaviorDetails();
            RebuildConditionDetails();
            RefreshButtons();
        }

        private void SetSelection(IStep selectedStep, IChapter selectedChapter, IProcess selectedProcess, bool forceRefresh)
        {
            bool changed = !ReferenceEquals(step, selectedStep)
                || !ReferenceEquals(chapter, selectedChapter)
                || !ReferenceEquals(process, selectedProcess);

            if (!changed && !forceRefresh)
            {
                return;
            }

            step = selectedStep;
            chapter = selectedChapter;
            process = selectedProcess;

            selectedBehaviorIndex = -1;
            selectedTransitionIndex = -1;
            selectedConditionIndex = -1;

            TryRefreshAll();
        }

        private void TryRefreshAll()
        {
            if (stepNameField == null
                || behaviorListView == null
                || transitionListView == null
                || conditionListView == null
                || transitionTargetPopup == null
                || behaviorDetails == null
                || conditionDetails == null)
            {
                return;
            }

            RefreshAll();
        }

        private void RefreshContext()
        {
            bool hasStep = step != null;

            noStepHelpBox.style.display = hasStep ? DisplayStyle.None : DisplayStyle.Flex;
            stepNameField.SetEnabled(hasStep);

            suppressCallbacks = true;
            stepNameField.SetValueWithoutNotify(hasStep ? step.Data.Name ?? string.Empty : string.Empty);
            suppressCallbacks = false;

            if (hasStep)
            {
                string processName = process?.Data?.Name ?? "<No Process>";
                string chapterName = chapter?.Data?.Name ?? "<No Chapter>";
                contextLabel.text = $"Process: {processName}   |   Chapter: {chapterName}";
            }
            else
            {
                contextLabel.text = "No step selected.";
            }
        }

        private void RefreshBehaviorList()
        {
            IList<IBehavior> list = GetBehaviors();
            lastBehaviorCount = list.Count;

            suppressCallbacks = true;
            behaviorListView.itemsSource = list as IList;
            behaviorListView.Rebuild();
            selectedBehaviorIndex = ClampIndex(selectedBehaviorIndex, list.Count);
            behaviorListView.selectedIndex = selectedBehaviorIndex;
            suppressCallbacks = false;
        }

        private void RefreshTransitionList()
        {
            IList<ITransition> list = GetTransitions();
            lastTransitionCount = list.Count;

            suppressCallbacks = true;
            transitionListView.itemsSource = list as IList;
            transitionListView.Rebuild();
            selectedTransitionIndex = ClampIndex(selectedTransitionIndex, list.Count);
            transitionListView.selectedIndex = selectedTransitionIndex;
            suppressCallbacks = false;
        }

        private void RefreshConditionList()
        {
            IList<ICondition> list = GetConditions();
            lastConditionCount = list.Count;

            suppressCallbacks = true;
            conditionListView.itemsSource = list as IList;
            conditionListView.Rebuild();
            selectedConditionIndex = ClampIndex(selectedConditionIndex, list.Count);
            conditionListView.selectedIndex = selectedConditionIndex;
            suppressCallbacks = false;
        }

        private void RefreshTransitionTargetPopup()
        {
            ITransition transition = GetSelectedTransition();
            List<string> choices = BuildTargetChoices(transition?.Data?.TargetStep);

            suppressCallbacks = true;
            transitionTargetPopup.choices = choices;
            transitionTargetPopup.SetValueWithoutNotify(choices.Count > 0 ? choices[0] : "<End Chapter>");
            transitionTargetPopup.SetEnabled(transition != null);
            suppressCallbacks = false;
        }

        private void ApplyStepName(string newName)
        {
            if (suppressCallbacks || step == null)
            {
                return;
            }

            string oldName = step.Data.Name ?? string.Empty;
            string normalized = newName ?? string.Empty;

            if (oldName == normalized)
            {
                return;
            }

            ExecuteMutation(
                () => step.Data.SetName(normalized),
                () => step.Data.SetName(oldName));
        }

        private void AddBehavior(IBehavior behavior)
        {
            if (step == null || behavior == null)
            {
                return;
            }

            IList<IBehavior> list = GetBehaviors();
            if (list == null)
            {
                return;
            }

            ExecuteMutation(
                () =>
                {
                    list.Add(behavior);
                    selectedBehaviorIndex = list.Count - 1;
                },
                () =>
                {
                    list.Remove(behavior);
                    selectedBehaviorIndex = ClampIndex(selectedBehaviorIndex, list.Count);
                });
        }

        private void RemoveSelectedBehavior()
        {
            IList<IBehavior> list = GetBehaviors();
            int index = selectedBehaviorIndex;

            if (!IsValidIndex(index, list.Count))
            {
                return;
            }

            IBehavior removed = list[index];
            ExecuteMutation(
                () =>
                {
                    list.RemoveAt(index);
                    selectedBehaviorIndex = ClampIndex(index, list.Count);
                },
                () =>
                {
                    list.Insert(index, removed);
                    selectedBehaviorIndex = index;
                });
        }

        private void MoveBehavior(int direction)
        {
            IList<IBehavior> list = GetBehaviors();
            int from = selectedBehaviorIndex;
            int to = from + direction;

            if (!IsValidIndex(from, list.Count) || !IsValidIndex(to, list.Count))
            {
                return;
            }

            ExecuteMutation(
                () =>
                {
                    MoveListItem(list, from, to);
                    selectedBehaviorIndex = to;
                },
                () =>
                {
                    MoveListItem(list, to, from);
                    selectedBehaviorIndex = from;
                });
        }

        private void AddTransition()
        {
            if (step == null)
            {
                return;
            }

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
            int index = selectedTransitionIndex;

            if (!IsValidIndex(index, list.Count))
            {
                return;
            }

            ITransition removed = list[index];
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

        private void MoveTransition(int direction)
        {
            IList<ITransition> list = GetTransitions();
            int from = selectedTransitionIndex;
            int to = from + direction;

            if (!IsValidIndex(from, list.Count) || !IsValidIndex(to, list.Count))
            {
                return;
            }

            ExecuteMutation(
                () =>
                {
                    MoveListItem(list, from, to);
                    selectedTransitionIndex = to;
                },
                () =>
                {
                    MoveListItem(list, to, from);
                    selectedTransitionIndex = from;
                });
        }

        private void AddCondition(ICondition condition)
        {
            if (step == null || condition == null || GetSelectedTransition() == null)
            {
                return;
            }

            IList<ICondition> list = GetConditions();
            if (list == null)
            {
                return;
            }

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
            int index = selectedConditionIndex;

            if (!IsValidIndex(index, list.Count))
            {
                return;
            }

            ICondition removed = list[index];
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

            if (!IsValidIndex(from, list.Count) || !IsValidIndex(to, list.Count))
            {
                return;
            }

            ExecuteMutation(
                () =>
                {
                    MoveListItem(list, from, to);
                    selectedConditionIndex = to;
                },
                () =>
                {
                    MoveListItem(list, to, from);
                    selectedConditionIndex = from;
                });
        }

        private void ApplyTransitionTarget(string selectedLabel)
        {
            if (suppressCallbacks)
            {
                return;
            }

            ITransition transition = GetSelectedTransition();
            if (transition == null)
            {
                return;
            }

            int selectedIndex = transitionTargetPopup.choices.IndexOf(selectedLabel);
            if (!IsValidIndex(selectedIndex, targetChoices.Count))
            {
                return;
            }

            IStep oldTarget = transition.Data.TargetStep;
            IStep newTarget = targetChoices[selectedIndex];

            if (ReferenceEquals(oldTarget, newTarget))
            {
                return;
            }

            ExecuteMutation(
                () => transition.Data.TargetStep = newTarget,
                () => transition.Data.TargetStep = oldTarget);
        }

        private List<string> BuildTargetChoices(IStep currentTarget)
        {
            List<string> labels = new List<string>();
            targetChoices.Clear();

            labels.Add("<End Chapter>");
            targetChoices.Add(null);

            IList<IStep> chapterSteps = chapter?.Data?.Steps;
            if (chapterSteps != null)
            {
                foreach (IStep candidate in chapterSteps)
                {
                    labels.Add(candidate?.Data?.Name ?? "<Unnamed Step>");
                    targetChoices.Add(candidate);
                }
            }

            int selectedIndex = 0;
            if (currentTarget != null)
            {
                int foundIndex = targetChoices.IndexOf(currentTarget);
                if (foundIndex >= 0)
                {
                    selectedIndex = foundIndex;
                }
            }

            if (selectedIndex > 0)
            {
                string selectedLabel = labels[selectedIndex];
                IStep selectedStep = targetChoices[selectedIndex];
                labels.RemoveAt(selectedIndex);
                targetChoices.RemoveAt(selectedIndex);
                labels.Insert(0, selectedLabel);
                targetChoices.Insert(0, selectedStep);
            }

            return labels;
        }

        private void RebuildBehaviorDetails()
        {
            behaviorDetails.Clear();

            IBehavior behavior = GetSelectedBehavior();
            if (behavior == null)
            {
                behaviorDetails.Add(new HelpBox("Select a behavior to edit.", HelpBoxMessageType.Info));
                return;
            }

            switch (behavior)
            {
                case DelayBehavior delay:
                    BuildDelayBehaviorDetails(delay);
                    break;
                case ConfettiBehavior confetti:
                    BuildConfettiBehaviorDetails(confetti);
                    break;
                default:
                    behaviorDetails.Add(new HelpBox("This prototype only supports Delay and Spawn Confetti behaviors.", HelpBoxMessageType.Warning));
                    break;
            }
        }

        private void BuildDelayBehaviorDetails(DelayBehavior behavior)
        {
            DelayBehavior.EntityData data = behavior.Data;

            FloatField delayField = new FloatField("Delay (in seconds)")
            {
                value = data.DelayTime,
            };
            delayField.RegisterValueChangedCallback(evt =>
            {
                float oldValue = data.DelayTime;
                float newValue = Mathf.Max(0f, evt.newValue);
                if (Mathf.Approximately(oldValue, newValue))
                {
                    return;
                }

                ExecuteMutation(
                    () => data.DelayTime = newValue,
                    () => data.DelayTime = oldValue);
            });

            behaviorDetails.Add(delayField);
        }

        private void BuildConfettiBehaviorDetails(ConfettiBehavior behavior)
        {
            ConfettiBehavior.EntityData data = behavior.Data;

            Toggle isAboveUserToggle = new Toggle("Spawn Above User")
            {
                value = data.IsAboveUser,
            };
            isAboveUserToggle.RegisterValueChangedCallback(evt =>
            {
                bool oldValue = data.IsAboveUser;
                bool newValue = evt.newValue;
                if (oldValue == newValue)
                {
                    return;
                }

                ExecuteMutation(
                    () => data.IsAboveUser = newValue,
                    () => data.IsAboveUser = oldValue);
            });
            behaviorDetails.Add(isAboveUserToggle);

            TextField prefabPathField = new TextField("Confetti Machine Path")
            {
                value = data.ConfettiMachinePrefabPath ?? string.Empty,
                isDelayed = true,
            };
            prefabPathField.RegisterValueChangedCallback(evt =>
            {
                string oldValue = data.ConfettiMachinePrefabPath ?? string.Empty;
                string newValue = evt.newValue ?? string.Empty;
                if (oldValue == newValue)
                {
                    return;
                }

                ExecuteMutation(
                    () => data.ConfettiMachinePrefabPath = newValue,
                    () => data.ConfettiMachinePrefabPath = oldValue);
            });
            behaviorDetails.Add(prefabPathField);

            FloatField areaRadiusField = new FloatField("Area Radius")
            {
                value = data.AreaRadius,
            };
            areaRadiusField.RegisterValueChangedCallback(evt =>
            {
                float oldValue = data.AreaRadius;
                float newValue = Mathf.Max(0f, evt.newValue);
                if (Mathf.Approximately(oldValue, newValue))
                {
                    return;
                }

                ExecuteMutation(
                    () => data.AreaRadius = newValue,
                    () => data.AreaRadius = oldValue);
            });
            behaviorDetails.Add(areaRadiusField);

            FloatField durationField = new FloatField("Duration")
            {
                value = data.Duration,
            };
            durationField.RegisterValueChangedCallback(evt =>
            {
                float oldValue = data.Duration;
                float newValue = Mathf.Max(0f, evt.newValue);
                if (Mathf.Approximately(oldValue, newValue))
                {
                    return;
                }

                ExecuteMutation(
                    () => data.Duration = newValue,
                    () => data.Duration = oldValue);
            });
            behaviorDetails.Add(durationField);

            EnumField executionStagesField = new EnumField("Execution Stages", data.ExecutionStages);
            executionStagesField.RegisterValueChangedCallback(evt =>
            {
                BehaviorExecutionStages oldValue = data.ExecutionStages;
                BehaviorExecutionStages newValue = (BehaviorExecutionStages)evt.newValue;
                if (oldValue == newValue)
                {
                    return;
                }

                ExecuteMutation(
                    () => data.ExecutionStages = newValue,
                    () => data.ExecutionStages = oldValue);
            });
            behaviorDetails.Add(executionStagesField);

            behaviorDetails.Add(new HelpBox("Position Provider reference editing is intentionally omitted in this mini prototype.", HelpBoxMessageType.Info));
        }

        private void RebuildConditionDetails()
        {
            conditionDetails.Clear();

            ICondition condition = GetSelectedCondition();
            if (condition == null)
            {
                conditionDetails.Add(new HelpBox("Select a condition to edit.", HelpBoxMessageType.Info));
                return;
            }

            switch (condition)
            {
                case TimeoutCondition timeout:
                    BuildTimeoutConditionDetails(timeout);
                    break;
                case ObjectInRangeCondition objectInRange:
                    BuildObjectInRangeConditionDetails(objectInRange);
                    break;
                default:
                    conditionDetails.Add(new HelpBox("This prototype only supports Timeout and Object Nearby conditions.", HelpBoxMessageType.Warning));
                    break;
            }
        }

        private void BuildTimeoutConditionDetails(TimeoutCondition condition)
        {
            TimeoutCondition.EntityData data = condition.Data;

            FloatField timeoutField = new FloatField("Wait (in seconds)")
            {
                value = data.Timeout,
            };
            timeoutField.RegisterValueChangedCallback(evt =>
            {
                float oldValue = data.Timeout;
                float newValue = Mathf.Max(0f, evt.newValue);
                if (Mathf.Approximately(oldValue, newValue))
                {
                    return;
                }

                ExecuteMutation(
                    () => data.Timeout = newValue,
                    () => data.Timeout = oldValue);
            });

            conditionDetails.Add(timeoutField);
        }

        private void BuildObjectInRangeConditionDetails(ObjectInRangeCondition condition)
        {
            ObjectInRangeCondition.EntityData data = condition.Data;

            FloatField rangeField = new FloatField("Range")
            {
                value = data.Range,
            };
            rangeField.RegisterValueChangedCallback(evt =>
            {
                float oldValue = data.Range;
                float newValue = Mathf.Max(0f, evt.newValue);
                if (Mathf.Approximately(oldValue, newValue))
                {
                    return;
                }

                ExecuteMutation(
                    () => data.Range = newValue,
                    () => data.Range = oldValue);
            });
            conditionDetails.Add(rangeField);

            FloatField requiredInsideField = new FloatField("Required seconds inside")
            {
                value = data.RequiredTimeInside,
            };
            requiredInsideField.RegisterValueChangedCallback(evt =>
            {
                float oldValue = data.RequiredTimeInside;
                float newValue = Mathf.Max(0f, evt.newValue);
                if (Mathf.Approximately(oldValue, newValue))
                {
                    return;
                }

                ExecuteMutation(
                    () => data.RequiredTimeInside = newValue,
                    () => data.RequiredTimeInside = oldValue);
            });
            conditionDetails.Add(requiredInsideField);

            conditionDetails.Add(new HelpBox("Tracked Object and Reference Object selection is intentionally omitted in this mini prototype.", HelpBoxMessageType.Info));
        }

        private void RefreshButtons()
        {
            bool hasStep = step != null;

            IList<IBehavior> behaviors = GetBehaviors();
            bool hasBehavior = hasStep && IsValidIndex(selectedBehaviorIndex, behaviors.Count);

            removeBehaviorButton.SetEnabled(hasBehavior);
            moveBehaviorUpButton.SetEnabled(hasBehavior && selectedBehaviorIndex > 0);
            moveBehaviorDownButton.SetEnabled(hasBehavior && selectedBehaviorIndex < behaviors.Count - 1);

            IList<ITransition> transitions = GetTransitions();
            bool hasTransition = hasStep && IsValidIndex(selectedTransitionIndex, transitions.Count);

            removeTransitionButton.SetEnabled(hasTransition);
            moveTransitionUpButton.SetEnabled(hasTransition && selectedTransitionIndex > 0);
            moveTransitionDownButton.SetEnabled(hasTransition && selectedTransitionIndex < transitions.Count - 1);
            transitionTargetPopup.SetEnabled(hasTransition);

            IList<ICondition> conditions = GetConditions();
            bool hasCondition = hasTransition && IsValidIndex(selectedConditionIndex, conditions.Count);

            removeConditionButton.SetEnabled(hasCondition);
            moveConditionUpButton.SetEnabled(hasCondition && selectedConditionIndex > 0);
            moveConditionDownButton.SetEnabled(hasCondition && selectedConditionIndex < conditions.Count - 1);
        }

        private void NotifyStepEdited()
        {
            if (step != null)
            {
                GlobalEditorHandler.CurrentStepModified(step);
                GlobalEditorHandler.CurrentProcessModified();
            }

            RefreshAll();
        }

        private void ExecuteMutation(Action doAction, Action undoAction)
        {
            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    doAction?.Invoke();
                    NotifyStepEdited();
                },
                () =>
                {
                    undoAction?.Invoke();
                    NotifyStepEdited();
                }));
        }

        private IList<IBehavior> GetBehaviors()
        {
            return step?.Data?.Behaviors?.Data?.Behaviors ?? Array.Empty<IBehavior>();
        }

        private IBehavior GetSelectedBehavior()
        {
            IList<IBehavior> list = GetBehaviors();
            return IsValidIndex(selectedBehaviorIndex, list.Count) ? list[selectedBehaviorIndex] : null;
        }

        private IList<ITransition> GetTransitions()
        {
            return step?.Data?.Transitions?.Data?.Transitions ?? Array.Empty<ITransition>();
        }

        private ITransition GetSelectedTransition()
        {
            IList<ITransition> list = GetTransitions();
            return IsValidIndex(selectedTransitionIndex, list.Count) ? list[selectedTransitionIndex] : null;
        }

        private IList<ICondition> GetConditions()
        {
            return GetSelectedTransition()?.Data?.Conditions ?? Array.Empty<ICondition>();
        }

        private ICondition GetSelectedCondition()
        {
            IList<ICondition> list = GetConditions();
            return IsValidIndex(selectedConditionIndex, list.Count) ? list[selectedConditionIndex] : null;
        }

        private static bool IsValidIndex(int index, int count)
        {
            return index >= 0 && index < count;
        }

        private static int ClampIndex(int index, int count)
        {
            if (count <= 0)
            {
                return -1;
            }

            return Mathf.Clamp(index, 0, count - 1);
        }

        private static void MoveListItem<T>(IList<T> list, int from, int to)
        {
            if (!IsValidIndex(from, list.Count) || !IsValidIndex(to, list.Count) || from == to)
            {
                return;
            }

            T item = list[from];
            list.RemoveAt(from);
            list.Insert(to, item);
        }

        private static string GetBehaviorTitle(IBehavior behavior, int index)
        {
            if (behavior?.Data == null)
            {
                return $"{index + 1}. <Null Behavior>";
            }

            string name = string.IsNullOrWhiteSpace(behavior.Data.Name)
                ? behavior.GetType().Name
                : behavior.Data.Name;

            return $"{index + 1}. {name}";
        }

        private static string GetTransitionTitle(ITransition transition, int index)
        {
            if (transition?.Data == null)
            {
                return $"{index + 1}. <Null Transition>";
            }

            string transitionName = string.IsNullOrWhiteSpace(transition.Data.Name)
                ? "(No conditions)"
                : transition.Data.Name;

            string targetName = transition.Data.TargetStep != null ? transition.Data.TargetStep.Data.Name : "<End Chapter>";
            return $"{index + 1}. {transitionName} -> {targetName}";
        }

        private static string GetConditionTitle(ICondition condition, int index)
        {
            if (condition?.Data == null)
            {
                return $"{index + 1}. <Null Condition>";
            }

            string name = string.IsNullOrWhiteSpace(condition.Data.Name)
                ? condition.GetType().Name
                : condition.Data.Name;

            return $"{index + 1}. {name}";
        }
    }
}
