using System;
using UnityEngine;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Interface for scene objects that can be highlighted.
    /// </summary>
    public interface IHighlightProperty : ISceneObjectProperty
    {
        /// <summary>
        /// Emitted when the object gets highlighted.
        /// </summary>
        event EventHandler<EventArgs> Highlighted;

        /// <summary>
        /// Emitted when the object gets unhighlighted.
        /// </summary>
        event EventHandler<EventArgs> Unhighlighted;

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
}
