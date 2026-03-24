using System;
using UnityEngine;
using UnityEngine.Events;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Abstract base property for color-based highlight implementations.
    /// </summary>
    /// <typeparam name="TEventArgs">Event args type emitted by this property.</typeparam>
    public abstract class AbstractColorHighlightProperty<TEventArgs> : ProcessSceneObjectProperty where TEventArgs : EventArgs
    {
        /// <summary>
        /// Is currently active.
        /// </summary>
        protected bool IsActive { get; private set; }

        /// <summary>
        /// Currently active color, if any.
        /// </summary>
        protected Color? CurrentColor { get; set; }

        /// <summary>
        /// Event that is emitted when highlighting starts.
        /// </summary>
        protected abstract UnityEvent<TEventArgs> StartedEvent { get; }

        /// <summary>
        /// Event that is emitted when highlighting ends.
        /// </summary>
        protected abstract UnityEvent<TEventArgs> EndedEvent { get; }

        /// <summary>
        /// Activates the visual highlight state.
        /// </summary>
        protected void Activate(Color color)
        {
            if (TryApplyVisualState(true, color) == false)
            {
                return;
            }

            IsActive = true;
            CurrentColor = color;

            StartedEvent?.Invoke(CreateEventArgs(GetStartedEventColor(color)));
        }

        /// <summary>
        /// Deactivates the visual highlight state.
        /// </summary>
        protected void Deactivate()
        {
            Color color = CurrentColor ?? default;

            if (TryApplyVisualState(false, color) == false)
            {
                return;
            }

            Color? eventColor = GetEndedEventColor(CurrentColor);

            IsActive = false;
            EndedEvent?.Invoke(CreateEventArgs(eventColor));
            CurrentColor = null;
        }

        /// <summary>
        /// Optionally transforms the event color emitted on highlight start.
        /// </summary>
        protected virtual Color? GetStartedEventColor(Color color)
        {
            return color;
        }

        /// <summary>
        /// Optionally transforms the event color emitted on highlight end.
        /// </summary>
        protected virtual Color? GetEndedEventColor(Color? color)
        {
            return color;
        }

        /// <summary>
        /// Applies the visual state.
        /// </summary>
        /// <param name="isActive"><c>true</c> when activating, <c>false</c> when deactivating.</param>
        /// <param name="color">Current highlight color.</param>
        /// <returns><c>true</c> if state change was applied.</returns>
        protected abstract bool TryApplyVisualState(bool isActive, Color color);

        /// <summary>
        /// Creates event args for the configured event color.
        /// </summary>
        protected abstract TEventArgs CreateEventArgs(Color? color);

    }

    /// <summary>
    /// Abstract base property for highlight properties.
    /// </summary>
    public abstract class BaseHighlightProperty : AbstractColorHighlightProperty<HighlightPropertyEventArgs>, IHighlightProperty
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
