using UnityEngine;

namespace VRBuilder.BasicInteraction.Validation
{
    /// <summary>
    /// Base validator used to implement concrete validators.
    /// </summary>
    public abstract class Validator : MonoBehaviour
    {
        /// <summary>
        /// Determines whether the specified object satisfies this validator's criteria.
        /// </summary>
        /// <param name="obj">The object to validate.</param>
        /// <returns>
        /// <see langword="true"/> if the object satisfies the validation criteria;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public abstract bool Validate(GameObject obj);

        private void OnEnable()
        {
            // Has to be implemented to allow disabling this script.
        }

        private void OnDisable()
        {
            // Has to be implemented to allow disabling this script.
        }
    }
}
