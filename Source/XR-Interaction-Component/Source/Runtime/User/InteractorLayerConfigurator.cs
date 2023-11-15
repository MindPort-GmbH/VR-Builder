using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VRBuilder.Core.Setup;

namespace VRBuilder.XRInteraction.User
{
    /// <summary>
    /// Configures interaction and/or raycast layers of a list of interactors to layers
    /// with the specified names.
    /// </summary>
    public class InteractorLayerConfigurator : MonoBehaviour, ISceneSetupComponent
    {
        [SerializeField]
        [Tooltip("Interactors to configure.")]
        private List<XRBaseInteractor> interactors = new List<XRBaseInteractor>();

        [SerializeField]
        [Tooltip("Name of the raycast layer.")]
        private string raycastLayerName;

        [SerializeField]
        [Tooltip("Name of the interaction layer.")]
        private string interactionLayerName;

        /// <inheritdoc/>
        public void ExecuteSetup()
        {
            foreach (XRBaseInteractor interactor in interactors)
            {
                SetupInteractionLayer(interactor);

                if (interactor is XRRayInteractor rayInteractor)
                {
                    SetupRaycastLayer(rayInteractor);
                }
            }
        }

        private void SetupInteractionLayer(XRBaseInteractor interactor)
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

            if (interactor.interactionLayers != interactionLayer)
            {
                interactor.interactionLayers = interactionLayer;
                EditorUtility.SetDirty(interactor);
                Debug.Log($"Interaction layer '{interactionLayerName}' has been updated to layer {interactionLayer.value} on interactor '{interactor.gameObject.name}'.");
            }
        }

        private void SetupRaycastLayer(XRRayInteractor interactor)
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

            if (interactor.raycastMask != raycastLayer)
            {
                interactor.raycastMask = raycastLayer;
                EditorUtility.SetDirty(interactor);
                Debug.Log($"Raycast layer '{raycastLayerName}' has been updated to layer {raycastLayer.value} on interactor '{interactor.gameObject.name}'.");
            }
        }
    }
}
