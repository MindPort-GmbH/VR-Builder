// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using UnityEngine.Events;
using VRBuilder.Core.Primitives;

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

        private Action<HighlightPropertyEventArgs> highlightStartedAction;
        private Action<HighlightPropertyEventArgs> highlightEndedAction;

        /// <inheritdoc/>
        public event Action<HighlightPropertyEventArgs> HighlightStartedAction
        {
            add => highlightStartedAction += value;
            remove => highlightStartedAction -= value;
        }

        /// <inheritdoc/>
        public event Action<HighlightPropertyEventArgs> HighlightEndedAction
        {
            add => highlightEndedAction += value;
            remove => highlightEndedAction -= value;
        }

        /// <summary>
        /// Is currently highlighted.
        /// </summary>
        public bool IsHighlighted => IsActive;

        /// <inheritdoc />
        protected override UnityEvent<HighlightPropertyEventArgs> StartedEvent => highlightStarted;

        /// <inheritdoc />
        protected override UnityEvent<HighlightPropertyEventArgs> EndedEvent => highlightEnded;

        protected override void OnEnable()
        {
            base.OnEnable();
            highlightStarted.AddListener(OnHighlightStarted);
            highlightEnded.AddListener(OnHighlightEnded);
        }

        private void OnHighlightStarted(HighlightPropertyEventArgs args)
        {
            highlightStartedAction?.Invoke(args);
        }

        private void OnHighlightEnded(HighlightPropertyEventArgs args)
        {
            highlightEndedAction?.Invoke(args);
        }

        /// <inheritdoc/>
        public virtual void Highlight(IColor highlightColor)
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
        protected abstract bool TryHighlight(IColor highlightColor);

        /// <summary>
        /// Applies the visual unhighlighted state.
        /// </summary>
        protected abstract bool TryUnhighlight();

        /// <inheritdoc />
        protected sealed override bool TryApplyVisualState(bool isActive, IColor color)
        {
            return isActive ? TryHighlight(color) : TryUnhighlight();
        }

        /// <inheritdoc />
        protected override IColor? GetEndedEventColor(IColor? color)
        {
            return null;
        }

        /// <inheritdoc />
        protected override HighlightPropertyEventArgs CreateEventArgs(IColor? color)
        {
            return new HighlightPropertyEventArgs(color);
        }
    }
}