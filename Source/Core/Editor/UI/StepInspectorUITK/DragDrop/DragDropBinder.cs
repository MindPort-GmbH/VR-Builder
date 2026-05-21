using System;
using System.Collections;
using System.Collections.Generic;
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
    ///
    /// Drag visuals are modeled on Unity's IMGUI ReorderableList:
    ///  * the dragged row is "lifted" (USS class <c>vrb-row--lifted</c>) and translated to
    ///    follow the cursor 1:1 (no transition).
    ///  * sibling rows in the drop range are translated by their own height to slide aside;
    ///    USS gives them a 120 ms ease-out transition so the slide is smooth.
    ///  * the insertion line is absolutely-positioned inside the rows container — moving it
    ///    just updates its inline <c>top</c>, never the DOM order, so rows below it never
    ///    reflow.
    ///
    /// Row layouts are snapshotted at drag commit and reused on every pointer move; we never
    /// call <c>worldBound</c> per move on rows whose own translate we are mutating (those
    /// reads would be unstable).
    /// </remarks>
    public static class DragDropBinder
    {
        private const float DragThresholdPx = 4f;
        private const string SourceActiveClass = "vrb-drag-source--active";
        private const string LiftedClass = "vrb-row--lifted";
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

            // Active-hover and overlay state.
            VisualElement currentHover = null;
            VisualElement insertionLine = null;

            // Drag-time layout cache. Snapshotted when the cursor enters a drop container so
            // every PointerMove avoids re-querying Yoga.
            VisualElement cachedContainer = null;
            VisualElement[] cachedRows = null;
            float[] cachedRowLocalY = null;
            float[] cachedRowHeight = null;
            float cachedContainerWorldY = 0f;

            HashSet<VisualElement> shiftedRows = new HashSet<VisualElement>();

            float dragStartPointerY = 0f;
            float lastHandledPointerY = float.NaN;

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
                    row.AddToClassList(LiftedClass);
                    dragStartPointerY = downPos.y;
                    lastHandledPointerY = float.NaN;

                    // Only NOW do we grab pointer capture — the user committed to a drag.
                    dragSource.CapturePointer(pointerId);
                    captured = true;
                }

                if (!DragSession.IsActive || dragSource.panel == null)
                {
                    return;
                }

                // Always translate the dragged row to follow the cursor, even if Y didn't
                // change — horizontal cursor jitter shouldn't desync the row.
                float liftDeltaY = evt.position.y - dragStartPointerY;
                row.style.translate = new StyleTranslate(new Translate(0f, liftDeltaY, 0f));

                // Cheap throttle: skip the rest if the cursor's Y barely moved.
                if (!float.IsNaN(lastHandledPointerY) && Mathf.Abs(evt.position.y - lastHandledPointerY) < 0.5f)
                {
                    return;
                }
                lastHandledPointerY = evt.position.y;

                VisualElement under = dragSource.panel.Pick(evt.position);
                var (container, target) = DropTargetRegistry.FindMatching(under, DragSession.Active.Kind);

                if (container != cachedContainer)
                {
                    // Container changed — reset old container's siblings, snapshot the new one.
                    ClearSiblingShifts();
                    SnapshotLayout(container, target);
                    cachedContainer = container;
                }

                UpdateHoverHighlight(container);
                UpdateDropPreview(container, target, evt.position);
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

                int dstIndex = -1;
                IList dstList = null;
                if (target != null)
                {
                    VisualElement[] rows = (container == cachedContainer && cachedRows != null)
                        ? cachedRows
                        : (target.GetRowElements?.Invoke() ?? Array.Empty<VisualElement>());
                    dstIndex = ComputeDropIndex(rows, evt.position.y, payload.SourceRow);
                    dstList = target.GetDropList?.Invoke();
                }

                // Snap visuals to a clean state BEFORE mutating the data model. The Lifted
                // class disables transitions, so clearing the source's translate is instant —
                // no animated rubberband-back.
                ClearInsertionLine();
                ClearSiblingShifts();
                ClearHoverHighlight();
                row.style.translate = new StyleTranslate(new Translate(0f, 0f, 0f));

                if (dstList != null)
                {
                    ListMoveCommand.Execute(
                        src: payload.SourceList,
                        srcIndex: payload.SourceIndex,
                        dst: dstList,
                        dstIndex: dstIndex,
                        item: payload.Item);
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
                lastHandledPointerY = float.NaN;

                ClearHoverHighlight();
                ClearInsertionLine();
                ClearSiblingShifts();
                row.style.translate = new StyleTranslate(new Translate(0f, 0f, 0f));
                row.RemoveFromClassList(LiftedClass);

                cachedContainer = null;
                cachedRows = null;
                cachedRowLocalY = null;
                cachedRowHeight = null;

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

            void SnapshotLayout(VisualElement container, DropTargetRegistry.DropTarget target)
            {
                if (container == null || target == null)
                {
                    cachedRows = null;
                    cachedRowLocalY = null;
                    cachedRowHeight = null;
                    cachedContainerWorldY = 0f;
                    return;
                }

                cachedRows = target.GetRowElements?.Invoke() ?? Array.Empty<VisualElement>();
                cachedRowLocalY = new float[cachedRows.Length];
                cachedRowHeight = new float[cachedRows.Length];
                cachedContainerWorldY = container.worldBound.y;

                for (int i = 0; i < cachedRows.Length; i++)
                {
                    Rect lb = cachedRows[i].layout;
                    cachedRowLocalY[i] = lb.y;
                    cachedRowHeight[i] = lb.height;
                }
            }

            void UpdateHoverHighlight(VisualElement newHover)
            {
                if (currentHover == newHover)
                {
                    return;
                }
                if (currentHover != null)
                {
                    currentHover.RemoveFromClassList(DropHoverClass);
                }
                currentHover = newHover;
                if (currentHover != null)
                {
                    currentHover.AddToClassList(DropHoverClass);
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

            void UpdateDropPreview(VisualElement container, DropTargetRegistry.DropTarget target, Vector2 position)
            {
                if (container == null || target == null || cachedRows == null)
                {
                    ClearInsertionLine();
                    ClearSiblingShifts();
                    return;
                }

                int dropIndex = ComputeDropIndexCached(position.y);
                int srcIndex = Array.IndexOf(cachedRows, row);

                ApplySiblingShifts(srcIndex, dropIndex);
                PositionInsertionLine(container, dropIndex);
            }

            int ComputeDropIndexCached(float pointerY)
            {
                if (cachedRows == null || cachedRows.Length == 0)
                {
                    return 0;
                }

                float localPointerY = pointerY - cachedContainerWorldY;

                for (int i = 0; i < cachedRows.Length; i++)
                {
                    if (cachedRows[i] == row)
                    {
                        continue;
                    }

                    float midY = cachedRowLocalY[i] + cachedRowHeight[i] * 0.5f;
                    if (localPointerY < midY)
                    {
                        return i;
                    }
                }
                return cachedRows.Length;
            }

            void ApplySiblingShifts(int srcIndex, int dropIndex)
            {
                HashSet<VisualElement> wanted = new HashSet<VisualElement>();

                if (cachedRows != null && srcIndex >= 0 && srcIndex < cachedRows.Length)
                {
                    if (dropIndex > srcIndex + 1)
                    {
                        // Drag down: rows between source and drop point slide UP to fill the gap.
                        for (int i = srcIndex + 1; i < dropIndex && i < cachedRows.Length; i++)
                        {
                            VisualElement r = cachedRows[i];
                            if (r == row) continue;
                            r.style.translate = new StyleTranslate(new Translate(0f, -cachedRowHeight[srcIndex], 0f));
                            wanted.Add(r);
                        }
                    }
                    else if (dropIndex < srcIndex)
                    {
                        // Drag up: rows between drop point and source slide DOWN to open a slot.
                        for (int i = dropIndex; i < srcIndex && i < cachedRows.Length; i++)
                        {
                            VisualElement r = cachedRows[i];
                            if (r == row) continue;
                            r.style.translate = new StyleTranslate(new Translate(0f, cachedRowHeight[srcIndex], 0f));
                            wanted.Add(r);
                        }
                    }
                }

                // Reset rows that were shifted last frame but aren't in the new wanted set.
                foreach (VisualElement r in shiftedRows)
                {
                    if (!wanted.Contains(r))
                    {
                        r.style.translate = new StyleTranslate(new Translate(0f, 0f, 0f));
                    }
                }
                shiftedRows = wanted;
            }

            void ClearSiblingShifts()
            {
                foreach (VisualElement r in shiftedRows)
                {
                    r.style.translate = new StyleTranslate(new Translate(0f, 0f, 0f));
                }
                shiftedRows.Clear();
            }

            void PositionInsertionLine(VisualElement container, int dropIndex)
            {
                EnsureInsertionLine();
                if (insertionLine.parent != container)
                {
                    insertionLine.RemoveFromHierarchy();
                    container.Add(insertionLine);
                }

                float lineY;
                if (cachedRows == null || cachedRows.Length == 0)
                {
                    lineY = 0f;
                }
                else if (dropIndex >= cachedRows.Length)
                {
                    int last = cachedRows.Length - 1;
                    lineY = cachedRowLocalY[last] + cachedRowHeight[last] - 1f;
                }
                else
                {
                    lineY = cachedRowLocalY[dropIndex] - 1f;
                }

                insertionLine.style.top = lineY;
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

        /// <summary>
        /// Fallback used at drop-time when the cache is empty (drop container was never
        /// hovered during the move phase). Reads <c>worldBound</c> directly.
        /// </summary>
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
