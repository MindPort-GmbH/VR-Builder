using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VRBuilder.Core.Properties;
using VRBuilder.BasicInteraction.Properties;
using System.Linq;

namespace VRBuilder.XRInteraction.Properties
{ 
    /// <summary>
    /// XR implementation of <see cref="ITouchableProperty"/>.
    /// </summary>
    [RequireComponent(typeof(InteractableObject))]
    public class TouchableProperty : LockableProperty, ITouchableProperty
    {
        public event EventHandler<EventArgs> Touched;
        public event EventHandler<EventArgs> Untouched;

        /// <summary>
        /// Returns true if the GameObject is touched.
        /// </summary>
        public virtual bool IsBeingTouched => Interactable != null && Interactable.interactorsHovering.Any(i => i.transform.root.GetComponentInChildren<UserSceneObject>() != null);

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
            
            Interactable.hoverEntered.AddListener(HandleXRTouched);
            Interactable.hoverExited.AddListener(HandleXRUntouched);

            InternalSetLocked(IsLocked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            Interactable.hoverEntered.RemoveListener(HandleXRTouched);
            Interactable.hoverExited.RemoveListener(HandleXRUntouched);
        }
        
        protected void Reset()
        {
            Interactable.IsTouchable = true;
            Interactable.IsGrabbable = GetComponent<GrabbableProperty>() != null;
            Interactable.IsUsable = GetComponent<UsableProperty>() != null;
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }

        private void HandleXRTouched(HoverEnterEventArgs arguments)
        {
            if (arguments.interactorObject.transform.root.GetComponentInChildren<UserSceneObject>() != null)
            {
                EmitTouched();
            }
        }

        private void HandleXRUntouched(HoverExitEventArgs arguments)
        {
            if (arguments.interactorObject.transform.root.GetComponentInChildren<UserSceneObject>() != null)
            {
                EmitUntouched();
            }            
        }

        protected void EmitTouched()
        {
            Touched?.Invoke(this, EventArgs.Empty);
        }

        protected void EmitUntouched()
        {
            Untouched?.Invoke(this, EventArgs.Empty);
        }

        protected override void InternalSetLocked(bool lockState)
        {
            Interactable.IsTouchable = lockState == false;
        }

        /// <inheritdoc />
        public void FastForwardTouch()
        {
            if (IsBeingTouched)
            {
                Interactable.ForceStopInteracting();
            }
            else
            {
                EmitTouched();
                EmitUntouched();
            }
        }
    }
}