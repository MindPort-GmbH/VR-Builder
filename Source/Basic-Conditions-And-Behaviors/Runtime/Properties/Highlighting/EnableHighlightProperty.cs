using UnityEngine;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Highlight property which enables a referenced <see cref="highlightObject"/>,
    /// which can be e.g. a colored copy of the original object or a marker.
    /// </summary>
    public class EnableHighlightProperty : BaseHighlightProperty
    {
        [Tooltip("Object to show for highlighting.")]
        [SerializeField]
        private GameObject highlightObject = null;

        public Color? CurrentHighlightColor { get; protected set; }

        /// <inheritdoc/>
        public override void Highlight(Color highlightColor)
        {
            if (highlightObject == null)
            {
                return;
            }

            CurrentHighlightColor = highlightColor;
            highlightObject.SetActive(true);
            IsHighlighted = true;

            EmitHighlightEvent(new HighlightPropertyEventArgs(CurrentHighlightColor));
        }

        /// <inheritdoc/>
        public override void Unhighlight()
        {
            if (highlightObject == null)
            {
                return;
            }

            CurrentHighlightColor = null;
            highlightObject.SetActive(false);
            IsHighlighted = false;

            EmitUnhighlightEvent(new HighlightPropertyEventArgs(CurrentHighlightColor));
        }
    }
}
