// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Decorations;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.DragDrop;
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

            VisualElement list = new VisualElement { name = "vrb-behaviors-list" };
            list.AddToClassList("vrb-drop-target");
            list.AddToClassList("vrb-drop-target--behaviors");
            root.Add(list);

            List<VisualElement> rows = new List<VisualElement>();
            for (int i = 0; i < behaviors.Count; i++)
            {
                int capturedIndex = i;
                IBehavior captured = behaviors[i];
                CollapsibleItem row = BuildBehaviorItem(captured, behaviors);

                DragDropBinder.MakeDraggable(
                    dragSource: row.Header,
                    row: row,
                    payloadFactory: () => new DragPayload(
                        DragKinds.Behavior, captured, (IList)behaviors, behaviors.IndexOf(captured), row));

                list.Add(row);
                rows.Add(row);
            }

            DragDropBinder.MakeDropTarget(
                container: list,
                acceptedKind: DragKinds.Behavior,
                getDropList: () => (IList)behaviors,
                getRowElements: () => rows.ToArray());

            root.Add(BuildAddButton(behaviors));
            return root;
        }

        public void Refresh() { }
        public void Dispose() { }

        private static CollapsibleItem BuildBehaviorItem(IBehavior behavior, IList<IBehavior> list)
        {
            CollapsibleItem item = new CollapsibleItem(
                title: EntityNaming.ResolveTitle(behavior),
                gripTooltip: Tooltips.Grip,
                deleteTooltip: Tooltips.DeleteBehavior,
                onDelete: () => RemoveBehavior(list, behavior),
                extraActions: EntityHeaderActions.BuildStandard(behavior, () => RemoveBehavior(list, behavior)),
                stateKey: behavior);

            item.Body.Add(BuildBehaviorBody(behavior));
            EntityDecorationRegistry.AppendApplicable(item.Body, behavior, onChanged: null);
            Validation.ValidationOverlay.DecorateEntityHeader(item.Header, behavior);
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
                tooltip = Tooltips.AddBehavior
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

    }
}
