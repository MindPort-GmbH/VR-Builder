using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core.Properties;

namespace VRBuilder.XRInteraction.Properties
{
    /// <summary>
    /// Reads poke depth from XRPokeFilter's pokeStateData every frame.
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable), typeof(XRPokeFilter))]
    public class PokableProperty : LockableProperty, IPokableProperty
    {
        [Header("Events")]
        [SerializeField]
        private UnityEvent<PokablePropertyEventArgs> pokeStarted = new UnityEvent<PokablePropertyEventArgs>();

        [SerializeField]
        private UnityEvent<PokablePropertyEventArgs> pokeEnded = new UnityEvent<PokablePropertyEventArgs>();

        private float currentPokeDepth;
        private XRSimpleInteractable simpleInteractable;
        private XRPokeFilter pokeFilter;

        /// <inheritdoc />
        public virtual bool IsBeingPoked { get; protected set; }

        /// <inheritdoc />
        public float CurrentPokeDepth => currentPokeDepth;

        /// <inheritdoc />
        public UnityEvent<PokablePropertyEventArgs> PokeStarted => pokeStarted;

        /// <inheritdoc />
        public UnityEvent<PokablePropertyEventArgs> PokeEnded => pokeEnded;

        protected XRSimpleInteractable Interactable
        {
            get
            {
                if (simpleInteractable == null)
                {
                    simpleInteractable = GetComponent<XRSimpleInteractable>();
                }

                return simpleInteractable;
            }
        }

        protected XRPokeFilter PokeFilter
        {
            get
            {
                if (pokeFilter == null)
                {
                    pokeFilter = GetComponent<XRPokeFilter>();
                }

                return pokeFilter;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            PokeFilter.pokeStateData?.Subscribe(OnPokeStateDataUpdated);
            Interactable.hoverExited.AddListener(OnHoverExited);

            InternalSetLocked(IsLocked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            PokeFilter.pokeStateData?.Unsubscribe(OnPokeStateDataUpdated);
            Interactable.hoverExited.RemoveListener(OnHoverExited);

            if (IsBeingPoked)
            {
                IsBeingPoked = false;
                currentPokeDepth = 0f;
                EmitUnpoked();
            }
        }

        protected override void Reset()
        {
            base.Reset();
            AutoSetup();
        }

        private void AutoSetup()
        {
            if (PokeFilter.pokeInteractable == null)
            {
                PokeFilter.pokeInteractable = Interactable;
            }

            if (PokeFilter.pokeCollider == null)
            {
                Collider collider = GetComponentInChildren<Collider>();

                if (collider != null)
                {
                    PokeFilter.pokeCollider = collider;
                }
                else
                {
                    Debug.LogWarning($"PokableProperty on '{gameObject.name}' needs a Collider. Add one and assign it to the XRPokeFilter.", this);
                }
            }

            Interactable.interactionLayers = 1;
        }

        private void OnPokeStateDataUpdated(PokeStateData data)
        {
            if (data.target == null)
            {
                currentPokeDepth = 0f;

                if (IsBeingPoked)
                {
                    IsBeingPoked = false;
                    EmitUnpoked();
                }

                return;
            }

            currentPokeDepth = 1f - data.interactionStrength;

            if (!IsBeingPoked)
            {
                IsBeingPoked = true;
                EmitPoked();
            }
        }

        private void OnHoverExited(HoverExitEventArgs args)
        {
            if (IsBeingPoked)
            {
                currentPokeDepth = 0f;
                IsBeingPoked = false;
                EmitUnpoked();
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
            Interactable.enabled = lockState == false;

            if (IsBeingPoked && lockState)
            {
                IsBeingPoked = false;
                currentPokeDepth = 0f;
                EmitUnpoked();
            }
        }

        /// <inheritdoc />
        public void ForceSetPokeState(bool isPoked, float depth)
        {
            currentPokeDepth = Mathf.Clamp01(depth);

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

        /// <inheritdoc />
        public void FastForwardPoke()
        {
            if (IsBeingPoked)
            {
                IsBeingPoked = false;
                currentPokeDepth = 0f;
                EmitUnpoked();
            }
            else
            {
                currentPokeDepth = 1f;
                EmitPoked();
                currentPokeDepth = 0f;
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
