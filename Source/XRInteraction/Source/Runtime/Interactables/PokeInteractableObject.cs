using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRBuilder.XRInteraction.Interactables
{
    /// <summary>
    /// Interactable component for poke-only objects.
    /// Extends XRSimpleInteractable and adds VR Builder control over the poke interaction.
    /// Does not require a Rigidbody. Should not coexist with InteractableObject on the same GameObject.
    /// </summary>
    [AddComponentMenu("VR Builder/Interactables/Poke Interactable Object (VR Builder)")]
    public class PokeInteractableObject : XRSimpleInteractable
    {
        [SerializeField]
        private bool isPokable = true;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Minimum poke interaction strength required before this object is considered poked.")]
        private float pokeActivationThreshold = 0.5f;

        /// <summary>
        /// Determines if this object can be poked.
        /// </summary>
        public bool IsPokable
        {
            get => isPokable;
            set => isPokable = value;
        }

        /// <summary>
        /// Minimum poke interaction strength required before this object is considered poked.
        /// </summary>
        public float PokeActivationThreshold
        {
            get => pokeActivationThreshold;
            set => pokeActivationThreshold = Mathf.Clamp01(value);
        }

        /// <summary>
        /// Only allows hover when poke is enabled.
        /// </summary>
        public override bool IsHoverableBy(IXRHoverInteractor interactor)
        {
            return isPokable && base.IsHoverableBy(interactor);
        }

        /// <summary>
        /// Poke objects cannot be selected. Always returns false.
        /// </summary>
        public override bool IsSelectableBy(IXRSelectInteractor interactor)
        {
            return false;
        }

        protected override void Reset()
        {
            base.Reset();

            isPokable = true;
            interactionLayers = 1;
        }

        /// <summary>
        /// Forces all hovering interactors to stop interacting for one frame.
        /// </summary>
        public virtual void ForceStopInteracting()
        {
            StartCoroutine(StopInteractingForOneFrame());
        }

        private void OnValidate()
        {
            pokeActivationThreshold = Mathf.Clamp01(pokeActivationThreshold);

            InteractableObject grabInteractable = GetComponent<InteractableObject>();

            if (grabInteractable != null)
            {
                Debug.LogWarning(
                    $"PokeInteractableObject on '{gameObject.name}' should not coexist with InteractableObject. " +
                    "Poke objects do not support grab or use interactions.", this);
            }
        }

        private IEnumerator StopInteractingForOneFrame()
        {
            bool wasPokable = isPokable;
            isPokable = false;

            yield return new WaitUntil(() => interactorsHovering.Count == 0);

            isPokable = wasPokable;
        }
    }
}
