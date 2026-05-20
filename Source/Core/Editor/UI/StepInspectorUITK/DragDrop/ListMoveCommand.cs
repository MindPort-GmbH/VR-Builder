using System;
using System.Collections;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows;
using VRBuilder.Core.Editor.UndoRedo;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.DragDrop
{
    /// <summary>
    /// Atomic remove-from-source + insert-into-destination. One <see cref="IRevertableCommand"/>
    /// covers reorder-within-list (src == dst with a different index) and cross-list moves
    /// (e.g. dragging a condition from transition A's list into transition B's).
    /// </summary>
    internal sealed class ListMoveCommand : IRevertableCommand
    {
        private readonly IList src;
        private readonly IList dst;
        private readonly object item;
        private readonly int srcIndex;
        private int dstIndex;

        public ListMoveCommand(IList src, int srcIndex, IList dst, int dstIndex, object item)
        {
            this.src = src;
            this.srcIndex = srcIndex;
            this.dst = dst;
            this.dstIndex = dstIndex;
            this.item = item;
        }

        public void Do()
        {
            if (srcIndex < 0 || srcIndex >= src.Count)
            {
                return;
            }

            src.RemoveAt(srcIndex);

            int insertAt = dstIndex;
            if (ReferenceEquals(src, dst) && insertAt > srcIndex)
            {
                insertAt--;
            }

            insertAt = Math.Clamp(insertAt, 0, dst.Count);
            dstIndex = insertAt;
            dst.Insert(insertAt, item);

            NotifyChanged();
        }

        public void Undo()
        {
            if (dstIndex < 0 || dstIndex >= dst.Count)
            {
                return;
            }

            dst.RemoveAt(dstIndex);

            int restoreAt = Math.Clamp(srcIndex, 0, src.Count);
            src.Insert(restoreAt, item);

            NotifyChanged();
        }

        public static void Execute(IList src, int srcIndex, IList dst, int dstIndex, object item)
        {
            // No-op if drag results in the same position.
            if (ReferenceEquals(src, dst) && (dstIndex == srcIndex || dstIndex == srcIndex + 1))
            {
                return;
            }

            RevertableChangesHandler.Do(new ListMoveCommand(src, srcIndex, dst, dstIndex, item));
        }

        private static void NotifyChanged()
        {
            IStep currentStep = StepSelectionService.CurrentStep;
            if (currentStep != null)
            {
                GlobalEditorHandler.CurrentStepModified(currentStep);
            }
            else
            {
                StepSelectionService.NotifyStepModified();
            }
        }
    }
}
