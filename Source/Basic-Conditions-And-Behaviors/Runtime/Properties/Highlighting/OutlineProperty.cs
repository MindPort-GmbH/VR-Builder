using UnityEngine;
using UnityEngine.Events;
using VRBuilder.Core.Utils.QuickOutline;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Property that allows an object to be highlighted with an outline.
    /// </summary>
    [RequireComponent(typeof(OutlineRenderer))]
    public class OutlineProperty : ProcessSceneObjectProperty, IOutlineProperty
    {
        [SerializeField]
        private UnityEvent<OutlinePropertyEventArgs> highlightStarted;

        [SerializeField]
        private UnityEvent<OutlinePropertyEventArgs> highlightEnded;

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
        public UnityEvent<OutlinePropertyEventArgs> OutlineStarted => highlightStarted;

        /// <inheritdoc/>
        public UnityEvent<OutlinePropertyEventArgs> OutlineEnded => highlightEnded;

        /// <inheritdoc/>
        public bool IsOutlined => OutlineRenderer.enabled;

        protected override void Reset()
        {
            OutlineRenderer.enabled = false;
            base.Reset();
        }

        /// <inheritdoc/>
        public void ShowOutline(Color highlightColor)
        {
            OutlineRenderer.OutlineColor = highlightColor;
            OutlineRenderer.enabled = true;
            OutlineStarted?.Invoke(new OutlinePropertyEventArgs(OutlineRenderer.OutlineColor));
        }

        /// <inheritdoc/>
        public void HideOutline()
        {
            OutlineRenderer.enabled = false;
            highlightEnded?.Invoke(new OutlinePropertyEventArgs(OutlineRenderer.OutlineColor));
        }
    }
}