using System;
using UnityEngine;
using UnityEngine.Events;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Base property for color-based highlight implementations.
    /// </summary>
    /// <typeparam name="TEventArgs">Event args type emitted by this property.</typeparam>
    public abstract class ColorHighlightPropertyBase<TEventArgs> : ProcessSceneObjectProperty where TEventArgs : EventArgs
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
}
