// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Decorations;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Instantiators;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows;
using VRBuilder.Core.Entities.Factories;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs
{
    internal sealed class TransitionsTab : IStepInspectorPanel
    {
        public string Id => PanelIds.Transitions;
        public GUIContent Label { get; } = new GUIContent("Transitions");

        public VisualElement BuildContent(IStepData step, IElementDrawerContext ctx)
        {
            ScrollView root = new ScrollView(ScrollViewMode.Vertical);
            root.AddToClassList("vrb-tab");
            root.AddToClassList("vrb-tab--transitions");

            IList<ITransition> transitions = step.Transitions?.Data?.Transitions;
            if (transitions == null)
            {
                root.Add(new Label("(no transitions collection)"));
                return root;
            }

            foreach (ITransition transition in transitions)
            {
                root.Add(BuildTransitionItem(transition, transitions));
            }

            root.Add(BuildAddTransitionButton(transitions));
            return root;
        }

        public void Refresh() { }
        public void Dispose() { }

        private static VisualElement BuildTransitionItem(ITransition transition, IList<ITransition> list)
        {
            CollapsibleItem item = new CollapsibleItem(
                title: ResolveTitle(transition),
                gripTooltip: "Drag to reorder (Phase 6)",
                deleteTooltip: "Remove this transition",
                onDelete: () => RemoveTransition(list, transition),
                extraActions: EntityHeaderActions.BuildStandard(transition, () => RemoveTransition(list, transition)));
            item.AddToClassList("vrb-item--transition");

            // Target step is intentionally not exposed as an inspector field — the link
            // is set by the process graph view's drag-out arrows.
            item.Body.Add(BuildConditionsList(transition));
            return item;
        }

        private static VisualElement BuildConditionsList(ITransition transition)
        {
            VisualElement section = new VisualElement();
            section.AddToClassList("vrb-conditions");

            Label header = new Label("Conditions");
            header.AddToClassList("vrb-conditions__header");
            section.Add(header);

            IList<ICondition> conditions = transition.Data.Conditions;
            foreach (ICondition condition in conditions)
            {
                section.Add(BuildConditionItem(condition, conditions));
            }

            section.Add(BuildAddConditionButton(conditions));
            return section;
        }

        private static VisualElement BuildConditionItem(ICondition condition, IList<ICondition> list)
        {
            CollapsibleItem item = new CollapsibleItem(
                title: condition?.Data?.Name ?? condition?.GetType().Name ?? "Condition",
                gripTooltip: "Drag to reorder or move to another transition (Phase 6)",
                deleteTooltip: "Remove this condition",
                onDelete: () => RemoveCondition(list, condition),
                extraActions: EntityHeaderActions.BuildStandard(condition, () => RemoveCondition(list, condition)));
            item.AddToClassList("vrb-item--condition");

            item.Body.Add(BuildConditionBody(condition));
            EntityDecorationRegistry.AppendApplicable(item.Body, condition, onChanged: null);
            return item;
        }

        private static VisualElement BuildConditionBody(ICondition condition)
        {
            if (condition?.Data == null) return new Label("(no data)");

            IElementDrawer drawer = ElementDrawerLocator.GetDrawerForValue(condition.Data, condition.Data.GetType());
            if (drawer == null)
            {
                return new Label("(no drawer for " + condition.Data.GetType().Name + ")");
            }

            // Inline edits self-update; don't trigger a rebuild that would kill active drags.
            return drawer.CreateElement(
                condition.Data,
                _ => { },
                new GUIContent(string.Empty));
        }

        private static Button BuildAddConditionButton(IList<ICondition> conditions)
        {
            Button button = new Button(() =>
            {
                AddMenuHelper.ShowMenu<ICondition>(
                    EditorConfigurator.Instance.ConditionsMenuContent,
                    newCondition =>
                    {
                        if (newCondition == null) return;

                        int index = conditions.Count;
                        TabMutations.Do(
                            () => conditions.Insert(index, newCondition),
                            () => conditions.RemoveAt(index));
                    });
            })
            {
                text = "+ Add Condition",
                tooltip = "Add a new condition to this transition"
            };
            button.AddToClassList("vrb-add-button");
            return button;
        }

        private static Button BuildAddTransitionButton(IList<ITransition> transitions)
        {
            Button button = new Button(() =>
            {
                ITransition newTransition = EntityFactory.CreateTransition();
                int index = transitions.Count;
                TabMutations.Do(
                    () => transitions.Insert(index, newTransition),
                    () => transitions.RemoveAt(index));
            })
            {
                text = "+ Add Transition",
                tooltip = "Add a new transition leaving this step"
            };
            button.AddToClassList("vrb-add-button");
            return button;
        }

        private static void RemoveTransition(IList<ITransition> list, ITransition transition)
        {
            int index = list.IndexOf(transition);
            if (index < 0) return;
            TabMutations.Do(
                () => list.RemoveAt(index),
                () => list.Insert(index, transition));
        }

        private static void RemoveCondition(IList<ICondition> list, ICondition condition)
        {
            int index = list.IndexOf(condition);
            if (index < 0) return;
            TabMutations.Do(
                () => list.RemoveAt(index),
                () => list.Insert(index, condition));
        }

        private static string ResolveTitle(ITransition transition)
        {
            if (transition?.Data == null) return "Transition";

            string target = transition.Data.TargetStep == null
                ? "End of Chapter"
                : (transition.Data.TargetStep.Data?.Name ?? "(unnamed step)");

            return $"Transition to \"{target}\"";
        }
    }
}
