using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRBuilder.XRInteraction.User
{
    public class InteractorLayerConfigurator : MonoBehaviour
    {
        [SerializeField]
        private List<XRBaseInteractor> rayInteractors = new List<XRBaseInteractor>();

        [SerializeField]
        private string raycastLayerName;

        public void Setup()
        {
            if (string.IsNullOrEmpty(raycastLayerName))
            {
                return;
            }

            foreach (XRBaseInteractor interactor in rayInteractors)
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
