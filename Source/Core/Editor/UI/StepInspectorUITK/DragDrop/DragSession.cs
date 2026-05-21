using System.Collections;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.DragDrop
{
    /// <summary>
    /// Snapshot of a drag-in-progress. Only one drag may be active at a time across the
    /// whole inspector — that single instance lives on <see cref="DragSession.Active"/>.
    /// </summary>
    public sealed class DragPayload
    {
        public string Kind { get; }
        public object Item { get; }
        public IList SourceList { get; }
        public int SourceIndex { get; }
        public VisualElement SourceRow { get; }

        public DragPayload(string kind, object item, IList sourceList, int sourceIndex, VisualElement sourceRow)
        {
            Kind = kind;
            Item = item;
            SourceList = sourceList;
            SourceIndex = sourceIndex;
            SourceRow = sourceRow;
        }
    }

    /// <summary>
    /// Hint published by <see cref="ListMoveCommand"/> immediately after a same-list
    /// reorder. Lets <c>DetachedPanelWindow.OnSelectionChanged</c> short-circuit the
    /// full panel rebuild and just move the existing row <c>VisualElement</c> in
    /// place — eliminates the post-drop flicker. Consumed (cleared) by whoever acts
    /// on it.
    /// </summary>
    public sealed class ReorderHint
    {
        public IList List { get; }
        public int FromIndex { get; }
        public int ToIndex { get; }

        public ReorderHint(IList list, int fromIndex, int toIndex)
        {
            List = list;
            FromIndex = fromIndex;
            ToIndex = toIndex;
        }
    }

    /// <summary>
    /// Tracks the one drag operation currently in flight (if any).
    /// </summary>
    public static class DragSession
    {
        public static DragPayload Active { get; private set; }

        /// <summary>
        /// After a drag ends UITK can still synthesize a <c>ClickEvent</c> on the element
        /// where the press started (e.g. the title Label inside a draggable header). That
        /// would wrongly toggle the foldout. <c>DragDropBinder</c> sets this true on drag
        /// end; consumers of clicks that double as toggles should call
        /// <see cref="ConsumeSuppressClick"/> at the top of their click handler.
        /// </summary>
        public static bool SuppressNextClick { get; set; }

        /// <summary>
        /// Set by <see cref="ListMoveCommand"/> when a same-list reorder just happened.
        /// Read + cleared by <c>DetachedPanelWindow</c> in the next selection-changed
        /// notification.
        /// </summary>
        public static ReorderHint PendingReorder { get; set; }

        public static void Begin(DragPayload payload) => Active = payload;
        public static void End() => Active = null;
        public static bool IsActive => Active != null;

        /// <summary>Returns true iff a drag just ended and the next click should be ignored.
        /// Resets the flag in the same call.</summary>
        public static bool ConsumeSuppressClick()
        {
            if (!SuppressNextClick) return false;
            SuppressNextClick = false;
            return true;
        }
    }
}
