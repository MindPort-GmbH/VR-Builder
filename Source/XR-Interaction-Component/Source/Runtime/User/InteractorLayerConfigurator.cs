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

        /// <inheritdoc/>
        public void ExecuteSetup()
        {
            if (string.IsNullOrEmpty(raycastLayerName))
            {
                return;
            }

            foreach (XRBaseInteractor interactor in interactors)
            {
                if (interactor is XRRayInteractor rayInteractor)
                {
                    SetupRaycastLayer(rayInteractor);
                }
            }
        }

        private void SetupRaycastLayer(XRRayInteractor interactor)
        {
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
                Debug.Log($"Raycast layer '{raycastLayerName}' has been updated to layer '{raycastLayer.value}' on interactor '{interactor.gameObject.name}'.");
            }
        }
    }
}
