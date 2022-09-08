using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VRBuilder.Core.Properties;
using VRBuilder.BasicInteraction.Properties;
using System.Linq;
using VRBuilder.Core.Settings;

namespace VRBuilder.XRInteraction.Properties
{
    /// <summary>
    /// XR implementation of <see cref="IGrabbableProperty"/>.
    /// </summary>
    [RequireComponent(typeof(TouchableProperty))]
    public class GrabbableProperty : LockableProperty, IGrabbableProperty
    {
        public event EventHandler<EventArgs> Grabbed;
        public event EventHandler<EventArgs> Ungrabbed;

        /// <summary>
        /// Returns true if the Interactable of this property is grabbed.
        /// </summary>
        public virtual bool IsGrabbed => Interactable != null && Interactable.isSelected && Interactable.interactorsSelecting.Any(interactor => interactor is XRSocketInteractor == false);

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

            Interactable.selectEntered.AddListener(HandleXRGrabbed);
            Interactable.selectExited.AddListener(HandleXRUngrabbed);

            InternalSetLocked(IsLocked);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
        
            Interactable.selectEntered.RemoveListener(HandleXRGrabbed);
            Interactable.selectExited.RemoveListener(HandleXRUngrabbed);
        }

        protected void Reset()
        {
            Interactable.IsGrabbable = true;
            GetComponent<Rigidbody>().isKinematic = InteractionSettings.Instance.MakeGrabbablesKinematic;
        }

        private void HandleXRGrabbed(SelectEnterEventArgs arguments)
        {
            if(arguments.interactorObject is XRSocketInteractor)
            {
                return;
            }

            EmitGrabbed();
        }

        private void HandleXRUngrabbed(SelectExitEventArgs arguments)
        {
            if (arguments.interactorObject is XRSocketInteractor)
            {
                return;
            }

            EmitUngrabbed();
        }

        protected void EmitGrabbed()
        {
            Grabbed?.Invoke(this, EventArgs.Empty);
        }

        protected void EmitUngrabbed()
        {
            Ungrabbed?.Invoke(this, EventArgs.Empty);
        }

        protected override void InternalSetLocked(bool lockState)
        {
            Interactable.IsGrabbable = lockState == false;
            
            if (IsGrabbed)
            {
                if (lockState)
                {
                    Interactable.ForceStopInteracting();
                }
            }
        }

        /// <summary>
        /// Instantaneously simulate that the object was grabbed.
        /// </summary>
        public void FastForwardGrab()
        {
            if (Interactable.isSelected)
            {
                StartCoroutine(UngrabGrabAndRelease());
            }
            else
            {
                EmitGrabbed();
                EmitUngrabbed();
            }
        }

        /// <summary>
        /// Instantaneously simulate that the object was ungrabbed.
        /// </summary>
        public void FastForwardUngrab()
        {
            if (Interactable.isSelected)
            {
                Interactable.ForceStopInteracting();
            }
            else
            {
                EmitGrabbed();
                EmitUngrabbed();
            }
        }

        private IEnumerator UngrabGrabAndRelease()
        {
            Interactable.ForceStopInteracting();
                
            yield return new WaitUntil(() => Interactable.isHovered == false && Interactable.isSelected == false);

            if (Interactable.interactorsSelecting.Count > 0)
            {
                DirectInteractor directInteractor = (DirectInteractor)Interactable.interactorsSelecting[0];
                directInteractor.AttemptGrab();

                yield return null;

                Interactable.ForceStopInteracting();
            }
        }
    }
}