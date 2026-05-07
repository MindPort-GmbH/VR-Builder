using System;
using UnityEngine;
using UnityEngine.Events;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Abstract base property for highlight properties.
    /// </summary>
    public abstract class BaseHighlightProperty : ColorHighlightPropertyBase<HighlightPropertyEventArgs>, IHighlightProperty
    {
        [Header("Events")]
        [SerializeField]
        private UnityEvent<HighlightPropertyEventArgs> highlightStarted = new UnityEvent<HighlightPropertyEventArgs>();

        [SerializeField]
        private UnityEvent<HighlightPropertyEventArgs> highlightEnded = new UnityEvent<HighlightPropertyEventArgs>();

        /// <summary>
        /// Event data for events of <see cref="BaseHighlightProperty"/>.
        /// </summary>
        public class HighlightEventArgs : EventArgs
        {
        }

        /// <summary>
        /// Is currently highlighted.
        /// </summary>
        public bool IsHighlighted => IsActive;

        /// <inheritdoc/>
        public UnityEvent<HighlightPropertyEventArgs> HighlightStarted => highlightStarted;

        /// <inheritdoc/>
        public UnityEvent<HighlightPropertyEventArgs> HighlightEnded => highlightEnded;

        /// <inheritdoc />
        protected override UnityEvent<HighlightPropertyEventArgs> StartedEvent => HighlightStarted;

        /// <inheritdoc />
        protected override UnityEvent<HighlightPropertyEventArgs> EndedEvent => HighlightEnded;

        /// <inheritdoc/>
        public virtual void Highlight(Color highlightColor)
        {
            Activate(highlightColor);
        }

        /// <inheritdoc/>
        public virtual void Unhighlight()
        {
            Deactivate();
        }

        /// <summary>
        /// Applies the visual highlighted state.
        /// </summary>
        protected abstract bool TryHighlight(Color highlightColor);

        /// <summary>
        /// Applies the visual unhighlighted state.
        /// </summary>
        protected abstract bool TryUnhighlight();

        /// <inheritdoc />
        protected sealed override bool TryApplyVisualState(bool isActive, Color color)
        {
            return isActive ? TryHighlight(color) : TryUnhighlight();
        }

        /// <inheritdoc />
        protected override Color? GetEndedEventColor(Color? color)
        {
            return null;
        }

        /// <inheritdoc />
        protected override HighlightPropertyEventArgs CreateEventArgs(Color? color)
        {
            return new HighlightPropertyEventArgs(color);
        }
    }
}
