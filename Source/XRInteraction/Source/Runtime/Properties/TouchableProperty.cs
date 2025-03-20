using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core.Properties;
using VRBuilder.XRInteraction.Interactables;

namespace VRBuilder.XRInteraction.Properties
{
    /// <summary>
    /// XR implementation of <see cref="ITouchableProperty"/>.
    /// </summary>
    [RequireComponent(typeof(InteractableObject), typeof(XRPokeFilter))]
    public class TouchableProperty : LockableProperty, ITouchableProperty
    {
        [Header("Events")]
        [SerializeField]
        private UnityEvent<TouchablePropertyEventArgs> touchStarted = new UnityEvent<TouchablePropertyEventArgs>();

        [SerializeField]
        private UnityEvent<TouchablePropertyEventArgs> touchEnded = new UnityEvent<TouchablePropertyEventArgs>();

        /// <summary>
        /// Returns true if the GameObject is touched.
        /// </summary>
        public virtual bool IsBeingTouched { get; protected set; }

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

        /// <summary>
        /// Reference to attached <see cref="XRPokeFilter"/>.
        /// </summary>
        protected XRPokeFilter PokeFilter
        {
            get
            {
                if (pokeFilter == false)
                {
                    pokeFilter = GetComponent<XRPokeFilter>();
                }

                return pokeFilter;
            }
        }

        private InteractableObject interactable;
        private XRPokeFilter pokeFilter;

        /// <inheritdoc />
        public UnityEvent<TouchablePropertyEventArgs> TouchStarted => touchStarted;

        /// <inheritdoc />
        public UnityEvent<TouchablePropertyEventArgs> TouchEnded => touchEnded;


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

            IsBeingTouched = false;
        }

        protected override void Reset()
        {
            base.Reset();
            SetComponentDefaultValues();

            if (PokeFilter.pokeCollider == null)
            {
                Debug.LogWarning($"TouchableProperty on {this.gameObject.name} requires a Collider assigned to the XRPokeFilter to work correctly.");
            }
        }

        private void SetComponentDefaultValues()
        {
            Interactable.IsTouchable = true;
            Interactable.IsGrabbable = GetComponent<GrabbableProperty>() != null;
            Interactable.IsUsable = GetComponent<UsableProperty>() != null;
            gameObject.GetComponent<Rigidbody>().isKinematic = true;

            PokeThresholdDatumProperty config = PokeFilter.pokeConfiguration;
            config.Value.pokeDirection = PokeAxis.None;
            config.Value.enablePokeAngleThreshold = false;
            PokeFilter.pokeConfiguration = config;
        }

        private void HandleXRTouched(HoverEnterEventArgs arguments)
        {
            if (arguments.interactorObject is XRPokeInteractor)
            {
                IsBeingTouched = true;
                EmitTouched();
            }
        }

        private void HandleXRUntouched(HoverExitEventArgs arguments)
        {
            if (arguments.interactorObject is XRPokeInteractor)
            {
                IsBeingTouched = false;
                EmitUntouched();
            }
        }

        protected void EmitTouched()
        {
            TouchStarted?.Invoke(new TouchablePropertyEventArgs());
        }

        protected void EmitUntouched()
        {
            TouchEnded?.Invoke(new TouchablePropertyEventArgs());
        }

        protected override void InternalSetLocked(bool lockState)
        {
            Interactable.IsTouchable = lockState == false;
            IsBeingTouched &= lockState == false;
        }

        /// <inheritdoc />
        public void ForceSetTouched(bool isTouched)
        {
            if (IsBeingTouched == isTouched)
            {
                return;
            }

            IsBeingTouched = isTouched;
            if (IsBeingTouched)
            {
                EmitTouched();
            }
            else
            {
                EmitUntouched();
            }
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