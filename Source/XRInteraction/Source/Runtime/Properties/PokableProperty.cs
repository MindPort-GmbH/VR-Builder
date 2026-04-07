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
    /// XR implementation of IPokableProperty.
    /// Uses PokeInteractableObject (extends XRSimpleInteractable) instead of InteractableObject (extends XRGrabInteractable),
    /// so no Rigidbody is needed. Works with XRPokeFilter to detect poke interactions.
    /// </summary>
    [RequireComponent(typeof(PokeInteractableObject), typeof(XRPokeFilter))]
    public class PokableProperty : LockableProperty, IPokableProperty
    {
        [Header("Events")]
        [SerializeField]
        private UnityEvent<PokablePropertyEventArgs> pokeStarted = new UnityEvent<PokablePropertyEventArgs>();

        [SerializeField]
        private UnityEvent<PokablePropertyEventArgs> pokeEnded = new UnityEvent<PokablePropertyEventArgs>();

        /// <summary>
        /// Returns true if this object is currently being poked.
        /// </summary>
        public virtual bool IsBeingPoked { get; protected set; }

        /// <inheritdoc />
        public UnityEvent<PokablePropertyEventArgs> PokeStarted => pokeStarted;

        /// <inheritdoc />
        public UnityEvent<PokablePropertyEventArgs> PokeEnded => pokeEnded;

        /// <summary>
        /// Reference to the attached PokeInteractableObject.
        /// </summary>
        protected PokeInteractableObject Interactable
        {
            get
            {
                if (interactable == false)
                {
                    interactable = GetComponent<PokeInteractableObject>();
                }

                return interactable;
            }
        }

        /// <summary>
        /// Reference to the attached XRPokeFilter.
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

        private PokeInteractableObject interactable;
        private XRPokeFilter pokeFilter;
        private int activePokeHoverCount;

        protected override void OnEnable()
        {
            base.OnEnable();

            Interactable.hoverEntered.AddListener(HandleXRPoked);
            Interactable.hoverExited.AddListener(HandleXRUnpoked);
            activePokeHoverCount = 0;
            IsBeingPoked = false;

            InternalSetLocked(IsLocked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Interactable.hoverEntered.RemoveListener(HandleXRPoked);
            Interactable.hoverExited.RemoveListener(HandleXRUnpoked);

            activePokeHoverCount = 0;
            IsBeingPoked = false;
        }

        protected override void Reset()
        {
            base.Reset();
            SetComponentDefaultValues();

            if (PokeFilter.pokeCollider == null)
            {
                Debug.LogWarning($"PokableProperty on '{gameObject.name}' requires a Collider assigned to the XRPokeFilter to work correctly.");
            }
        }

        private void SetComponentDefaultValues()
        {
            Interactable.IsPokable = true;

            if (PokeFilter.pokeInteractable == null)
            {
                PokeFilter.pokeInteractable = Interactable;
            }

            PokeThresholdDatumProperty config = PokeFilter.pokeConfiguration;
            config.Value.pokeDirection = PokeAxis.None;
            config.Value.enablePokeAngleThreshold = false;
            PokeFilter.pokeConfiguration = config;
        }

        private void HandleXRPoked(HoverEnterEventArgs arguments)
        {
            if (arguments.interactorObject is XRPokeInteractor)
            {
                activePokeHoverCount++;
                EvaluatePokeState();
            }
        }

        private void HandleXRUnpoked(HoverExitEventArgs arguments)
        {
            if (arguments.interactorObject is XRPokeInteractor)
            {
                activePokeHoverCount = Mathf.Max(0, activePokeHoverCount - 1);
                EvaluatePokeState();
            }
        }

        protected void EmitPoked()
        {
            PokeStarted?.Invoke(new PokablePropertyEventArgs());
        }

        protected void EmitUnpoked()
        {
            PokeEnded?.Invoke(new PokablePropertyEventArgs());
        }

        protected override void InternalSetLocked(bool lockState)
        {
            Interactable.IsPokable = lockState == false;

            if (lockState)
            {
                activePokeHoverCount = 0;
                IsBeingPoked = false;
            }
        }

        /// <inheritdoc />
        public void ForceSetPoked(bool isPoked)
        {
            SetPokeState(isPoked);
        }

        /// <inheritdoc />
        public void FastForwardPoke()
        {
            if (IsBeingPoked)
            {
                Interactable.ForceStopInteracting();
            }
            else
            {
                EmitPoked();
                EmitUnpoked();
            }
        }

        private void Update()
        {
            if (activePokeHoverCount > 0)
            {
                EvaluatePokeState();
            }
        }

        private void EvaluatePokeState()
        {
            if (activePokeHoverCount == 0)
            {
                SetPokeState(false);
                return;
            }

            float interactionStrength = 0f;
            if (PokeFilter.pokeStateData != null)
            {
                interactionStrength = Mathf.Clamp01(PokeFilter.pokeStateData.Value.interactionStrength);
            }

            SetPokeState(interactionStrength >= Interactable.PokeActivationThreshold);
        }

        private void SetPokeState(bool isPoked)
        {
            if (IsBeingPoked == isPoked)
            {
                return;
            }

            IsBeingPoked = isPoked;

            if (IsBeingPoked)
            {
                EmitPoked();
            }
            else
            {
                EmitUnpoked();
            }
        }

        private void OnValidate()
        {
            if (GetComponent<GrabbableProperty>() != null)
            {
                Debug.LogWarning(
                    $"PokableProperty on '{gameObject.name}' should not coexist with GrabbableProperty. " +
                    "Poke objects do not support grab interactions.", this);
            }

            if (GetComponent<UsableProperty>() != null)
            {
                Debug.LogWarning(
                    $"PokableProperty on '{gameObject.name}' should not coexist with UsableProperty. " +
                    "Poke objects do not support use interactions.", this);
            }

            if (GetComponent<TouchableProperty>() != null)
            {
                Debug.LogWarning(
                    $"PokableProperty on '{gameObject.name}' should not coexist with TouchableProperty. " +
                    "Use either PokableProperty or TouchableProperty, not both.", this);
            }
        }
    }
}
