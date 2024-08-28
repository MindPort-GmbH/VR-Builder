using System;
using UnityEngine;
using UnityEngine.Events;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Interface for scene objects that can be highlighted with an outline.
    /// </summary>
    public interface IOutlineHighlightProperty : ISceneObjectProperty
    {
        /// <summary>
        /// Emitted when the object gets highlighted.
        /// </summary>
        UnityEvent<OutlineHighlightPropertyEventArgs> HighlightStarted { get; }

        /// <summary>
        /// Emitted when the object gets unhighlighted.
        /// </summary>
        UnityEvent<OutlineHighlightPropertyEventArgs> HighlightEnded { get; }

        /// <summary>
        /// Is object currently highlighted.
        /// </summary>
        bool IsHighlighted { get; }

        /// <summary>
        /// Highlight this object and use <paramref name="highlightColor"/>.
        /// </summary>
        /// <param name="highlightColor">Color to use for highlighting.</param>
        void Highlight(Color highlightColor);

        /// <summary>
        /// Disable highlight.
        /// </summary>
        void Unhighlight();
    }

    public class OutlineHighlightPropertyEventArgs : EventArgs
    {
        public readonly Color? HighlightColor;

        public OutlineHighlightPropertyEventArgs(Color? highlightColor)
        {
            HighlightColor = highlightColor;
        }
    }
}
