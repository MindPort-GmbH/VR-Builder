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

        /// <inheritdoc/>
        protected override bool TryHighlight(Color highlightColor)
        {
            if (highlightObject == null)
            {
                return false;
            }

            highlightObject.SetActive(true);
            return true;
        }

        /// <inheritdoc/>
        protected override bool TryUnhighlight()
        {
            if (highlightObject == null)
            {
                return false;
            }

            highlightObject.SetActive(false);
            return true;
        }
    }
}
