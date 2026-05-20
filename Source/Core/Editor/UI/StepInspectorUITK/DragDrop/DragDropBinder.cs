using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.DragDrop
{
    /// <summary>
    /// Wires drag-source / drop-target behavior on existing visual elements.
    /// </summary>
    /// <remarks>
    /// The drag source is typically the whole row header (grip + caret area + title), not
    /// just the grip. PointerDown on a child <see cref="Button"/> is ignored so the action
    /// buttons (delete / menu / help / caret) keep their normal click semantics. Pointer
    /// capture is only acquired AFTER the cursor has moved past the drag threshold, which
    /// keeps brief clicks routing as clicks (the foldout caret/title toggle continues to
    /// work even though they live inside the drag-source).
    /// </remarks>
    public static class DragDropBinder
    {
        private const float DragThresholdPx = 4f;
        private const string SourceActiveClass = "vrb-drag-source--active";
        private const string DropHoverClass = "vrb-drop-target--hover";
        private const string InsertionLineClass = "vrb-drop-target__insertion-line";

        /// <summary>
        /// Marks <paramref name="dragSource"/> as the drag-initiator for <paramref name="row"/>.
        /// </summary>
        public static void MakeDraggable(
            VisualElement dragSource,
            VisualElement row,
            Func<DragPayload> payloadFactory)
        {
            if (dragSource == null || row == null || payloadFactory == null)
            {
                return;
            }

            Vector2 downPos = default;
            bool pointerDown = false;
            bool captured = false;
            int pointerId = -1;

            VisualElement currentHover = null;
            VisualElement insertionLine = null;

            dragSource.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0)
                {
                    return;
                }

                // Ignore presses on action buttons — they should fire their own clicks.
                if (IsInteractiveChild(evt.target as VisualElement, dragSource))
                {
                    return;
                }

                downPos = evt.position;
                pointerDown = true;
                pointerId = evt.pointerId;
                captured = false;
                // Do not capture yet — a non-dragged click should bubble to the caret/title.
            });

            dragSource.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!pointerDown)
                {
                    return;
                }

                if (!DragSession.IsActive)
                {
                    Vector2 delta = (Vector2)evt.position - downPos;
                    if (delta.sqrMagnitude < DragThresholdPx * DragThresholdPx)
                    {
                        return;
                    }

                    DragPayload payload = payloadFactory();
                    if (payload == null)
                    {
                        return;
                    }

                    DragSession.Begin(payload);
                    row.AddToClassList(SourceActiveClass);

                    // Only NOW do we grab pointer capture — the user committed to a drag.
                    dragSource.CapturePointer(pointerId);
                    captured = true;
                }

                if (!DragSession.IsActive || dragSource.panel == null)
                {
                    return;
                }

                VisualElement under = dragSource.panel.Pick(evt.position);
                var (container, target) = DropTargetRegistry.FindMatching(under, DragSession.Active.Kind);

                UpdateHoverHighlight(container);
                UpdateInsertionLine(container, target, evt.position);
            });

            dragSource.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (!DragSession.IsActive)
                {
                    EndDrag();
                    return;
                }

                DragPayload payload = DragSession.Active;
                VisualElement under = dragSource.panel?.Pick(evt.position);
                var (container, target) = DropTargetRegistry.FindMatching(under, payload.Kind);

                if (target != null)
                {
                    VisualElement[] rows = target.GetRowElements?.Invoke() ?? Array.Empty<VisualElement>();
                    int dstIndex = ComputeDropIndex(rows, evt.position.y, payload.SourceRow);
                    IList dstList = target.GetDropList?.Invoke();
                    if (dstList != null)
                    {
                        ListMoveCommand.Execute(
                            src: payload.SourceList,
                            srcIndex: payload.SourceIndex,
                            dst: dstList,
                            dstIndex: dstIndex,
                            item: payload.Item);
                    }
                }

                EndDrag();
            });

            dragSource.RegisterCallback<PointerCaptureOutEvent>(_ => EndDrag());

            void EndDrag()
            {
                if (captured && dragSource.HasPointerCapture(pointerId))
                {
                    dragSource.ReleasePointer(pointerId);
                }
                pointerDown = false;
                captured = false;

                ClearHoverHighlight();
                ClearInsertionLine();

                if (DragSession.IsActive)
                {
                    row.RemoveFromClassList(SourceActiveClass);
                    DragSession.End();

                    // UITK can synthesize a ClickEvent on the element where the press
                    // started after we release. That would wrongly toggle the foldout —
                    // mark the next click as suppressed.
                    DragSession.SuppressNextClick = true;
                }
            }

            void UpdateHoverHighlight(VisualElement newHover)
            {
                if (currentHover == newHover)
                {
                    return;
                }
                ClearHoverHighlight();
                if (newHover != null)
                {
                    newHover.AddToClassList(DropHoverClass);
                    currentHover = newHover;
                }
            }

            void ClearHoverHighlight()
            {
                if (currentHover != null)
                {
                    currentHover.RemoveFromClassList(DropHoverClass);
                    currentHover = null;
                }
            }

            void UpdateInsertionLine(VisualElement container, DropTargetRegistry.DropTarget target, Vector2 position)
            {
                if (container == null || target == null)
                {
                    ClearInsertionLine();
                    return;
                }

                EnsureInsertionLine();

                VisualElement[] rows = target.GetRowElements?.Invoke() ?? Array.Empty<VisualElement>();
                int dropIndex = ComputeDropIndex(rows, position.y, row);

                if (insertionLine.parent != container)
                {
                    insertionLine.RemoveFromHierarchy();
                    container.Add(insertionLine);
                }

                int targetIndexInContainer;
                if (rows.Length == 0)
                {
                    targetIndexInContainer = container.childCount - 1;
                }
                else if (dropIndex >= rows.Length)
                {
                    targetIndexInContainer = container.IndexOf(rows[rows.Length - 1]) + 1;
                }
                else
                {
                    targetIndexInContainer = container.IndexOf(rows[dropIndex]);
                }

                targetIndexInContainer = Math.Clamp(targetIndexInContainer, 0, container.childCount);
                container.Insert(targetIndexInContainer, insertionLine);
            }

            void EnsureInsertionLine()
            {
                if (insertionLine != null)
                {
                    return;
                }

                insertionLine = new VisualElement();
                insertionLine.AddToClassList(InsertionLineClass);
                insertionLine.pickingMode = PickingMode.Ignore;
            }

            void ClearInsertionLine()
            {
                if (insertionLine != null)
                {
                    insertionLine.RemoveFromHierarchy();
                }
            }
        }

        public static void MakeDropTarget(
            VisualElement container,
            string acceptedKind,
            Func<IList> getDropList,
            Func<VisualElement[]> getRowElements)
        {
            if (container == null || acceptedKind == null || getDropList == null)
            {
                return;
            }

            DropTargetRegistry.Register(container,
                new DropTargetRegistry.DropTarget(acceptedKind, getDropList, getRowElements));
        }

        private static bool IsInteractiveChild(VisualElement target, VisualElement dragSource)
        {
            VisualElement current = target;
            while (current != null && current != dragSource)
            {
                if (current is Button)
                {
                    return true;
                }
                current = current.parent;
            }
            return false;
        }

        private static int ComputeDropIndex(VisualElement[] rows, float pointerY, VisualElement sourceRow)
        {
            if (rows == null || rows.Length == 0)
            {
                return 0;
            }

            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] == sourceRow)
                {
                    continue;
                }

                Rect worldRect = rows[i].worldBound;
                float midY = worldRect.y + worldRect.height * 0.5f;
                if (pointerY < midY)
                {
                    return i;
                }
            }

            return rows.Length;
        }
    }
}
