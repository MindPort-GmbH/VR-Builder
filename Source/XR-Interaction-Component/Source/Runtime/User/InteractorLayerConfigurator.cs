using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using VRBuilder.Core.Setup;

namespace VRBuilder.XRInteraction.User
{
    /// <summary>
    /// Configures interaction and/or raycast layers of a list of interactors to layers
    /// with the specified names.
    /// </summary>
    public class InteractorLayerConfigurator : MonoBehaviour, ILayerConfigurator
    {
        [SerializeField]
        [Tooltip("Specifies the set of layers to use on this configurator.")]
        private LayerSet layerSet = LayerSet.None;

        [SerializeField]
        [Tooltip("Interactors to configure.")]
        private List<XRBaseInteractor> interactors = new List<XRBaseInteractor>();

        /// <inheritdoc/>
        public LayerSet LayerSet => layerSet;

        /// <inheritdoc/>
        public void ConfigureLayers(string interactionLayerName, string raycastLayerName)
        {
            foreach (XRBaseInteractor interactor in interactors)
            {
                SetupInteractionLayer(interactor, interactionLayerName);

                if (interactor is XRRayInteractor rayInteractor)
                {
                    SetupRaycastLayer(rayInteractor, raycastLayerName);
                }
            }
        }

        private void SetupInteractionLayer(XRBaseInteractor interactor, string interactionLayerName)
        {
            if (string.IsNullOrEmpty(interactionLayerName))
            {
                return;
            }

            InteractionLayerMask interactionLayer = InteractionLayerMask.NameToLayer(interactionLayerName);

            if (interactionLayer.value < 0)
            {
                Debug.LogError($"Layer '{interactionLayerName}' does not exist.");
                return;
            }

            if (interactor.interactionLayers != 1 << interactionLayer.value)
            {
                interactor.interactionLayers = 1 << interactionLayer.value;
                Debug.Log($"[{gameObject.name}] Interaction layer '{interactionLayerName}' has been updated to layer {interactionLayer.value} on interactor '{interactor.gameObject.name}'.");
            }
        }

        private void SetupRaycastLayer(XRRayInteractor interactor, string raycastLayerName)
        {
            if (string.IsNullOrEmpty(raycastLayerName))
            {
                return;
            }

            LayerMask raycastLayer = LayerMask.NameToLayer(raycastLayerName);

            if (raycastLayer.value < 0)
            {
                Debug.LogError($"Layer '{raycastLayerName}' does not exist.");
                return;
            }

            if (interactor.raycastMask != 1 << raycastLayer.value)
            {
                interactor.raycastMask = 1 << raycastLayer.value;
                Debug.Log($"[{gameObject.name}] Raycast layer '{raycastLayerName}' has been updated to layer {raycastLayer.value} on interactor '{interactor.gameObject.name}'.");
            }
        }
    }
}
