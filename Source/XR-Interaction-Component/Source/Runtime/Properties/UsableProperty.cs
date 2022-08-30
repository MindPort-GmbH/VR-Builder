using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VRBuilder.Core.Properties;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core.Settings;

namespace VRBuilder.XRInteraction.Properties
{
    /// <summary>
    /// XR implementation of <see cref="IUsableProperty"/>.
    /// </summary>
    [RequireComponent(typeof(GrabbableProperty))]
    public class UsableProperty : LockableProperty, IUsableProperty
    {
        public event EventHandler<EventArgs> UsageStarted;
        public event EventHandler<EventArgs> UsageStopped;

        /// <summary>
        /// Returns true if the GameObject is being used.
        /// </summary>
        public virtual bool IsBeingUsed => Interactable != null && Interactable.IsActivated;

        /// <summary>
        /// Reference to attached <see cref="InteractableObject"/>.
        /// </summary>
        protected InteractableObject Interactable
        {
            get
            {
                if (interactable == false)
                {
                    interactable = GetComponent<InteractableObject>();
                }

                return interactable;
            }
        }

        private InteractableObject interactable;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            Interactable.activated.AddListener(HandleXRUsageStarted);
            Interactable.deactivated.AddListener(HandleXRUsageStopped);
            
            InternalSetLocked(IsLocked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Interactable.activated.RemoveListener(HandleXRUsageStarted);
            Interactable.deactivated.RemoveListener(HandleXRUsageStopped);
        }

        protected void Reset()
        {
            Interactable.IsUsable = true;
            gameObject.GetComponent<Rigidbody>().isKinematic = InteractionSettings.Instance.MakeGrabbablesKinematic;
        }

        private void HandleXRUsageStarted(ActivateEventArgs arguments)
        {
            EmitUsageStarted();
        }

        private void HandleXRUsageStopped(DeactivateEventArgs arguments)
        {
            EmitUsageStopped();
        }

        protected void EmitUsageStarted()
        {
            UsageStarted?.Invoke(this, EventArgs.Empty);
        }

        protected void EmitUsageStopped()
        {
            UsageStopped?.Invoke(this, EventArgs.Empty);
        }

        protected override void InternalSetLocked(bool lockState)
        {
            Interactable.IsUsable = lockState == false;
            
            if (IsBeingUsed)
            {
                if (lockState)
                {
                    Interactable.ForceStopInteracting();
                }
            }
        }

        /// <summary>
        /// Instantaneously simulate that the object was used.
        /// </summary>
        public void FastForwardUse()
        {
            if (IsBeingUsed)
            {
                Interactable.ForceStopInteracting();
            }
            else
            {
                EmitUsageStarted();
                EmitUsageStopped();
            }
        }
    }
}