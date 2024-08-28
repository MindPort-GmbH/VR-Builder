using UnityEngine;
using UnityEngine.Events;
using VRBuilder.Core.Utils.QuickOutline;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Property that allows an object to be highlighted with an outline.
    /// </summary>
    [RequireComponent(typeof(OutlineRenderer))]
    public class OutlineHighlightProperty : ProcessSceneObjectProperty, IOutlineHighlightProperty
    {
        [SerializeField]
        private UnityEvent<OutlineHighlightPropertyEventArgs> highlightStarted;

        [SerializeField]
        private UnityEvent<OutlineHighlightPropertyEventArgs> highlightEnded;

        private OutlineRenderer outlineRenderer;

        /// <summary>
        /// The <see cref="OutlineRenderer"/> associated with this property.
        /// </summary>
        protected OutlineRenderer OutlineRenderer
        {
            get
            {
                if (outlineRenderer == null)
                {
                    outlineRenderer = GetComponent<OutlineRenderer>();
                }

                return outlineRenderer;
            }
        }

        /// <inheritdoc/>
        public UnityEvent<OutlineHighlightPropertyEventArgs> HighlightStarted => highlightStarted;

        /// <inheritdoc/>
        public UnityEvent<OutlineHighlightPropertyEventArgs> HighlightEnded => highlightEnded;

        /// <inheritdoc/>
        public bool IsHighlighted => OutlineRenderer.enabled;

        protected override void Reset()
        {
            OutlineRenderer.enabled = false;
            base.Reset();
        }

        /// <inheritdoc/>
        public void Highlight(Color highlightColor)
        {
            OutlineRenderer.OutlineColor = highlightColor;
            OutlineRenderer.enabled = true;
            HighlightStarted?.Invoke(new OutlineHighlightPropertyEventArgs(OutlineRenderer.OutlineColor));
        }

        /// <inheritdoc/>
        public void Unhighlight()
        {
            OutlineRenderer.enabled = false;
            highlightEnded?.Invoke(new OutlineHighlightPropertyEventArgs(OutlineRenderer.OutlineColor));
        }
    }
}