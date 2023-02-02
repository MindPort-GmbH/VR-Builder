using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace VRBuilder.Networking
{
    public class PlayerCameraEnabler : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            InputActionManager inputActionManager = GetComponentInChildren<InputActionManager>();

            if (IsOwner == false)
            {
                Debug.Log($"Disabling input for rig {OwnerClientId}");
                inputActionManager.DisableInput();

                foreach(XRBaseController controller in GetComponentsInChildren<XRBaseController>())
                {
                    Destroy(controller);
                }

                foreach(TrackedPoseDriver driver in GetComponentsInChildren<TrackedPoseDriver>())
                {
                    Destroy(driver);
                }

                foreach (Camera camera in GetComponentsInChildren<Camera>())
                {
                    Destroy(camera.GetComponent<FlareLayer>());
                    Destroy(camera.GetComponent<AudioListener>());
                    Destroy(camera);

                    foreach (MeshRenderer meshRenderer in camera.GetComponentsInChildren<MeshRenderer>())
                    {
                        meshRenderer.enabled = true;
                    }
                }
            }
        }
    }
}
