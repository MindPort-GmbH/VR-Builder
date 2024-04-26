using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRBuilder.XRInteraction
{
    /// <summary>
    /// Interactor used for directly interacting with interactables that are touching. This is handled via trigger volumes
    /// that update the current set of valid targets for this interactor. This component must have a collision volume that is 
    /// set to be a trigger to work.
    /// </summary>
    /// <remarks>Adds extra control over applicable interactions.</remarks>
    public partial class DirectInteractor : UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor
    {

        private bool forceGrab;

        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// Gets whether the selection state is active for this interactor. This will check if the controller has a valid selection
        /// state or whether toggle selection is currently on and active.
        /// </summary>
        public override bool isSelectActive
        {
            get
            {
                if (forceGrab)
                {
                    forceGrab = false;
                    return true;
                }

                return base.isSelectActive;
            }
        }

        /// <summary>
        /// Attempts to grab an interactable hovering this interactor without needing to press the grab button on the controller.
        /// </summary>
        public virtual void AttemptGrab()
        {
            forceGrab = true;
        }
    }
}
