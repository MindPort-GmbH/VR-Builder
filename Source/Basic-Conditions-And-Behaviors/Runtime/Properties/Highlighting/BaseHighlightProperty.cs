using System;
using UnityEngine;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Abstract base property for highlight properties.
    /// </summary>
    public abstract class BaseHighlightProperty : ProcessSceneObjectProperty, IHighlightProperty
    {
        /// <summary>
        /// Event data for events of <see cref="BaseHighlightProperty"/>.
        /// </summary>
        public class HighlightEventArgs : EventArgs { }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> Highlighted;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> Unhighlighted;

        /// <summary>
        /// Is currently highlighted.
        /// </summary>
        public bool IsHighlighted { get; protected set; }

        /// <inheritdoc/>
        public abstract void Highlight(Color highlightColor);

        /// <inheritdoc/>
        public abstract void Unhighlight();

        /// <summary>
        /// Emits an event when the property is highlighted.
        /// </summary>
        public void EmitHighlightEvent()
        {
            if (Highlighted != null)
            {
                Highlighted.Invoke(this, new HighlightEventArgs());
            }
        }

        /// <summary>
        /// Emits an event when the property is unhighlighted.
        /// </summary>
        public void EmitUnhighlightEvent()
        {
            if (Unhighlighted != null)
            {
                Unhighlighted.Invoke(this, new HighlightEventArgs());
            }
        }
    }
}
