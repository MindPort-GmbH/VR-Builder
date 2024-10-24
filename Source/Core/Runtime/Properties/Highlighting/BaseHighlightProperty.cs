using System;
using UnityEngine;
using UnityEngine.Events;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Abstract base property for highlight properties.
    /// </summary>
    public abstract class BaseHighlightProperty : ProcessSceneObjectProperty, IHighlightProperty
    {
        [Header("Events")]
        [SerializeField]
        private UnityEvent<HighlightPropertyEventArgs> highlightStarted = new UnityEvent<HighlightPropertyEventArgs>();

        [SerializeField]
        private UnityEvent<HighlightPropertyEventArgs> highlightEnded = new UnityEvent<HighlightPropertyEventArgs>();

        /// <summary>
        /// Event data for events of <see cref="BaseHighlightProperty"/>.
        /// </summary>
        public class HighlightEventArgs : EventArgs { }

        /// <summary>
        /// Is currently highlighted.
        /// </summary>
        public bool IsHighlighted { get; protected set; }

        /// <inheritdoc/>
        public UnityEvent<HighlightPropertyEventArgs> HighlightStarted => highlightStarted;

        /// <inheritdoc/>
        public UnityEvent<HighlightPropertyEventArgs> HighlightEnded => highlightEnded;

        /// <inheritdoc/>
        public abstract void Highlight(Color highlightColor);

        /// <inheritdoc/>
        public abstract void Unhighlight();

        /// <summary>
        /// Emits an event when the property is highlighted.
        /// </summary>
        public void EmitHighlightEvent(HighlightPropertyEventArgs args)
        {
            HighlightStarted?.Invoke(args);
        }

        /// <summary>
        /// Emits an event when the property is unhighlighted.
        /// </summary>
        public void EmitUnhighlightEvent(HighlightPropertyEventArgs args)
        {
            HighlightEnded?.Invoke(args);
        }
    }
}
