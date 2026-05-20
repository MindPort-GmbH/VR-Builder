using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Decorations;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.DragDrop;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers
{
    /// <summary>
    /// Default drawer for <see cref="IList"/> values. Renders one child drawer per entry
    /// in a vertical stack. Entries that are themselves <see cref="IEntity"/> instances
    /// (behaviors, conditions, transitions) get wrapped in a <see cref="CollapsibleItem"/>
    /// so nested lists — for example inside a <c>BehaviorSequence</c> — match the look of
    /// the top-level tabs.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(IList))]
    internal class ListElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            VisualElement root = new VisualElement();
            root.AddToClassList("vrb-list");

            if (value == null)
            {
                Label nullLabel = new Label(label?.text ?? "(empty list)") { tooltip = label?.tooltip };
                nullLabel.AddToClassList("vrb-list__null");
                root.Add(nullLabel);
                return root;
            }

            // The list header used to be rendered here, but in practice the surrounding
            // member (Foldable wrapper or parent ObjectElementDrawer) already prints the
            // member name — printing it again produced a duplicate "Child behaviors" line.

            IList list = (IList)value;
            Type entryDeclaredType = ReflectionUtils.GetEntryType(list);

            VisualElement rowsContainer = new VisualElement();
            rowsContainer.AddToClassList("vrb-list__rows");
            root.Add(rowsContainer);

            List<VisualElement> entityRows = new List<VisualElement>();
            // Determine the drag kind from the declared element type up front so an EMPTY
            // list still registers a drop target — otherwise the user can't drag a behavior
            // into a freshly-created Behavior Sequence.
            string entityDragKind = ResolveDragKindForType(entryDeclaredType);
            if (entityDragKind != null)
            {
                // Give the container a minimum height so the cursor can actually land on it
                // when the list is empty.
                rowsContainer.style.minHeight = 18;
            }

            for (int i = 0; i < list.Count; i++)
            {
                int closuredIndex = i;
                object entry = list[closuredIndex];

                IElementDrawer entryDrawer = ElementDrawerLocator.GetDrawerForValue(entry, entryDeclaredType);
                if (entryDrawer == null)
                {
                    continue;
                }

                Action<object> entryChanged = newValue =>
                {
                    if (closuredIndex >= list.Count)
                    {
                        ReflectionUtils.InsertIntoList(ref list, Math.Min(closuredIndex, list.Count), newValue);
                    }
                    else
                    {
                        list[closuredIndex] = newValue;
                    }

                    if (newValue == null)
                    {
                        ReflectionUtils.RemoveFromList(ref list, closuredIndex);
                    }

                    changeCallback(list);
                };

                VisualElement row;
                if (entry is IEntity entity)
                {
                    CollapsibleItem item = BuildEntityRow(entity, list, entryDrawer, entryChanged);
                    row = item;

                    string kind = ResolveDragKind(entity) ?? entityDragKind;
                    if (kind != null)
                    {
                        IEntity capturedEntity = entity;
                        CollapsibleItem capturedItem = item;
                        string capturedKind = kind;
                        DragDropBinder.MakeDraggable(
                            dragSource: capturedItem.Header,
                            row: capturedItem,
                            payloadFactory: () =>
                            {
                                int currentIndex = list.IndexOf(capturedEntity);
                                if (currentIndex < 0) return null;
                                return new DragPayload(capturedKind, capturedEntity, list, currentIndex, capturedItem);
                            });
                        entityRows.Add(item);
                    }
                }
                else
                {
                    row = BuildPlainRow(entry, entryDeclaredType, entryDrawer, entryChanged);
                }

                if (row != null)
                {
                    row.AddToClassList("vrb-list__item");
                    rowsContainer.Add(row);
                }
            }

            // Register the container as a drop target so cross-list moves (e.g. dragging a
            // behavior from the top-level tab into a nested BehaviorSequence) work too.
            if (entityDragKind != null)
            {
                rowsContainer.AddToClassList("vrb-drop-target");
                rowsContainer.AddToClassList("vrb-drop-target--nested");

                if (entityRows.Count == 0)
                {
                    Label emptyHint = new Label(BuildEmptyHint(entityDragKind));
                    emptyHint.AddToClassList("vrb-list__empty-hint");
                    emptyHint.pickingMode = PickingMode.Ignore;
                    rowsContainer.Add(emptyHint);
                }

                IList capturedList = list;
                List<VisualElement> capturedRows = entityRows;
                DragDropBinder.MakeDropTarget(
                    container: rowsContainer,
                    acceptedKind: entityDragKind,
                    getDropList: () => capturedList,
                    getRowElements: () => capturedRows.ToArray());
            }

            return root;
        }

        private static string BuildEmptyHint(string entityDragKind)
        {
            return entityDragKind switch
            {
                DragKinds.Behavior => "Drag or Add Behaviors here",
                DragKinds.Condition => "Drag or Add Conditions here",
                _ => "Drag or Add items here"
            };
        }

        private static VisualElement BuildPlainRow(object entry, Type entryDeclaredType, IElementDrawer entryDrawer, Action<object> entryChanged)
        {
            GUIContent entryLabel = entryDrawer.GetLabel(entry, entryDeclaredType);
            return entryDrawer.CreateElement(entry, entryChanged, entryLabel);
        }

        private static string ResolveDragKind(IEntity entity)
        {
            return entity switch
            {
                Core.Behaviors.IBehavior => DragKinds.Behavior,
                Core.Conditions.ICondition => DragKinds.Condition,
                _ => null
            };
        }

        private static string ResolveDragKindForType(Type entryDeclaredType)
        {
            if (entryDeclaredType == null) return null;

            if (typeof(Core.Behaviors.IBehavior).IsAssignableFrom(entryDeclaredType))
            {
                return DragKinds.Behavior;
            }

            if (typeof(Core.Conditions.ICondition).IsAssignableFrom(entryDeclaredType))
            {
                return DragKinds.Condition;
            }

            return null;
        }

        private static CollapsibleItem BuildEntityRow(IEntity entity, IList list, IElementDrawer entryDrawer, Action<object> entryChanged)
        {
            CollapsibleItem item = new CollapsibleItem(
                title: EntityNaming.ResolveTitle(entity),
                gripTooltip: Tooltips.Grip,
                deleteTooltip: ResolveDeleteTooltip(entity),
                onDelete: () => RemoveEntity(list, entity),
                extraActions: EntityHeaderActions.BuildStandard(entity, () => RemoveEntity(list, entity)),
                stateKey: entity);

            // Use the entry drawer to render the data body. The label is intentionally empty
            // because the header above already shows the entity title.
            VisualElement body = entryDrawer.CreateElement(entity, entryChanged, new GUIContent(string.Empty));
            if (body != null)
            {
                item.Body.Add(body);
            }

            // Wait-for-completion toggle and other decorations come from the same registry
            // that top-level behavior rows use.
            EntityDecorationRegistry.AppendApplicable(item.Body, entity, onChanged: null);

            Validation.ValidationOverlay.DecorateEntityHeader(item.Header, entity);

            return item;
        }

        private static string ResolveDeleteTooltip(IEntity entity)
        {
            return entity switch
            {
                Core.Behaviors.IBehavior => Tooltips.DeleteBehavior,
                Core.Conditions.ICondition => Tooltips.DeleteCondition,
                _ => "Remove this entry"
            };
        }

        private static void RemoveEntity(IList list, IEntity entity)
        {
            int index = list.IndexOf(entity);
            if (index < 0) return;

            TabMutations.Do(
                () => list.RemoveAt(index),
                () => list.Insert(index, entity));
        }
    }
}
