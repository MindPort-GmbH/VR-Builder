using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace VRBuilder.XRInteraction.Properties
{
    /// <summary>
    /// Extends <see cref="XRPokeFollowAffordance"/> with completion threshold logic based on follow travel.
    /// </summary>
    [AddComponentMenu("VR Builder/Affordances/Poke Follow Threshold Affordance")]
    [DisallowMultipleComponent]
    public class PokeFollowThresholdAffordance : XRPokeFollowAffordance
    {
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Normalized follow distance required before this poke is considered complete (0 = initial, 1 = max distance).")]
        private float pokeCompletedThreshold = 0.95f;

        private XRPokeFilter pokeFilter;

        /// <summary>
        /// Normalized follow distance required before this poke is considered complete.
        /// </summary>
        public float PokeCompletedThreshold
        {
            get => pokeCompletedThreshold;
            set => pokeCompletedThreshold = Mathf.Clamp01(value);
        }

        /// <summary>
        /// Current normalized poke progress based on follow travel, with interaction-strength fallback.
        /// </summary>
        public float NormalizedPokeProgress
        {
            get
            {
                if (pokeFollowTransform != null && maxDistance > Mathf.Epsilon)
                {
                    float pushedDistance = Vector3.Distance(initialPosition, pokeFollowTransform.localPosition);
                    return Mathf.Clamp01(pushedDistance / maxDistance);
                }

                return GetInteractionStrengthFallback();
            }
        }

        /// <summary>
        /// Whether poke progress has reached the configured completion threshold.
        /// </summary>
        public bool IsPokeCompleted => NormalizedPokeProgress >= pokeCompletedThreshold;

        private float GetInteractionStrengthFallback()
        {
            if (pokeFilter == null)
            {
                pokeFilter = GetComponentInParent<XRPokeFilter>();
            }

            if (pokeFilter != null && pokeFilter.pokeStateData != null)
            {
                return Mathf.Clamp01(pokeFilter.pokeStateData.Value.interactionStrength);
            }

            return 0f;
        }

        private void OnValidate()
        {
            pokeCompletedThreshold = Mathf.Clamp01(pokeCompletedThreshold);
        }
    }
}
