// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Decorations;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Instantiators;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs
{
    internal sealed class BehaviorsTab : IStepInspectorPanel
    {
        public string Id => PanelIds.Behaviors;
        public GUIContent Label { get; } = new GUIContent("Behaviors");

        public VisualElement BuildContent(IStepData step, IElementDrawerContext ctx)
        {
            ScrollView root = new ScrollView(ScrollViewMode.Vertical);
            root.AddToClassList("vrb-tab");
            root.AddToClassList("vrb-tab--behaviors");

            IList<IBehavior> behaviors = step.Behaviors?.Data?.Behaviors;
            if (behaviors == null)
            {
                root.Add(new Label("(no behaviors collection)"));
                return root;
            }

            foreach (IBehavior behavior in behaviors)
            {
                root.Add(BuildBehaviorItem(behavior, behaviors));
            }

            root.Add(BuildAddButton(behaviors));
            return root;
        }

        public void Refresh() { }
        public void Dispose() { }

        private static VisualElement BuildBehaviorItem(IBehavior behavior, IList<IBehavior> list)
        {
            CollapsibleItem item = new CollapsibleItem(
                title: ResolveTitle(behavior),
                onDelete: () => RemoveBehavior(list, behavior),
                gripTooltip: "Drag to reorder (Phase 6)",
                deleteTooltip: "Remove this behavior");

            item.Body.Add(BuildBehaviorBody(behavior));
            EntityDecorationRegistry.AppendApplicable(item.Body, behavior, onChanged: null);
            return item;
        }

        private static VisualElement BuildBehaviorBody(IBehavior behavior)
        {
            if (behavior?.Data == null)
            {
                return new Label("(no data)");
            }

            IElementDrawer drawer = ElementDrawerLocator.GetDrawerForValue(behavior.Data, behavior.Data.GetType());
            if (drawer == null)
            {
                return new Label("(no drawer for " + behavior.Data.GetType().Name + ")");
            }

            // Inline edits commit through ChangeValue → RevertableChangesHandler and the field
            // self-updates. We deliberately don't trigger a panel rebuild here — that would
            // destroy any active drag interaction (e.g. a slider).
            return drawer.CreateElement(
                behavior.Data,
                _ => { },
                new GUIContent(string.Empty));
        }

        private static Button BuildAddButton(IList<IBehavior> behaviors)
        {
            Button button = new Button(() =>
            {
                AddMenuHelper.ShowMenu<IBehavior>(
                    EditorConfigurator.Instance.BehaviorsMenuContent,
                    newBehavior =>
                    {
                        if (newBehavior == null) return;

                        int index = behaviors.Count;
                        TabMutations.Do(
                            () => behaviors.Insert(index, newBehavior),
                            () => behaviors.RemoveAt(index));
                    });
            })
            {
                text = "+ Add Behavior",
                tooltip = "Add a new behavior to this step"
            };
            button.AddToClassList("vrb-add-button");
            return button;
        }

        private static void RemoveBehavior(IList<IBehavior> list, IBehavior behavior)
        {
            int index = list.IndexOf(behavior);
            if (index < 0) return;

            TabMutations.Do(
                () => list.RemoveAt(index),
                () => list.Insert(index, behavior));
        }

        private static string ResolveTitle(IBehavior behavior)
        {
            if (behavior?.Data == null) return "Behavior";

            object[] attrs = behavior.Data.GetType().GetCustomAttributes(typeof(DisplayNameAttribute), true);
            if (attrs.Length > 0 && attrs[0] is DisplayNameAttribute attr && string.IsNullOrEmpty(attr.Name) == false)
            {
                return attr.Name;
            }

            if (behavior.Data is INamedData named && string.IsNullOrEmpty(named.Name) == false)
            {
                return named.Name;
            }

            return behavior.GetType().Name;
        }
    }
}
