using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.DragDrop
{
    /// <summary>
    /// Static lookup of drop-target metadata keyed by the container <see cref="VisualElement"/>.
    /// </summary>
    /// <remarks>
    /// During a drag the grip holds pointer capture, so the drop targets never receive their
    /// own pointer events. Instead we register targets here and the grip walks up from
    /// <c>panel.Pick(position)</c> looking for a match.
    /// </remarks>
    internal static class DropTargetRegistry
    {
        public sealed class DropTarget
        {
            public string AcceptedKind { get; }
            public Func<IList> GetDropList { get; }
            public Func<VisualElement[]> GetRowElements { get; }

            public DropTarget(string acceptedKind, Func<IList> getDropList, Func<VisualElement[]> getRowElements)
            {
                AcceptedKind = acceptedKind;
                GetDropList = getDropList;
                GetRowElements = getRowElements;
            }
        }

        private static readonly Dictionary<VisualElement, DropTarget> registered = new Dictionary<VisualElement, DropTarget>();

        public static void Register(VisualElement container, DropTarget target)
        {
            if (container == null || target == null)
            {
                return;
            }

            registered[container] = target;

            // Auto-unregister so a window close / panel rebuild doesn't leak entries.
            container.RegisterCallback<DetachFromPanelEvent>(_ => Unregister(container));
        }

        public static void Unregister(VisualElement container)
        {
            if (container == null)
            {
                return;
            }
            registered.Remove(container);
        }

        /// <summary>
        /// Walks up the parent chain from <paramref name="leaf"/> looking for a registered drop
        /// target whose <see cref="DropTarget.AcceptedKind"/> matches <paramref name="kind"/>.
        /// </summary>
        public static (VisualElement Container, DropTarget Target) FindMatching(VisualElement leaf, string kind)
        {
            VisualElement current = leaf;
            while (current != null)
            {
                if (registered.TryGetValue(current, out DropTarget target) && target.AcceptedKind == kind)
                {
                    return (current, target);
                }
                current = current.parent;
            }
            return (null, null);
        }
    }
}
