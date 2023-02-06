using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using VRBuilder.XRInteraction;

namespace VRBuilder.Networking
{
    public class NetworkUser : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner == false && IsClient)
            {
                foreach (ActionBasedControllerManager manager in GetComponentsInChildren<ActionBasedControllerManager>())
                {
                    Destroy(manager);
                }

                //Debug.Log($"Disabling input for rig {OwnerClientId}");
                //inputActionManager.DisableInput();

                foreach (XRBaseController controller in GetComponentsInChildren<XRBaseController>())
                {
                    controller.enableInputActions = false;
                    controller.enableInputTracking= false;
                }

                foreach (TrackedPoseDriver driver in GetComponentsInChildren<TrackedPoseDriver>())
                {
                    Destroy(driver);
                }

                //foreach(XRBaseInteractor interactor in GetComponentsInChildren<XRBaseInteractor>())
                //{
                //    Destroy(interactor);
                //}

                foreach (Camera camera in GetComponentsInChildren<Camera>())
                {
                    Destroy(camera.GetComponent<FlareLayer>());
                    Destroy(camera.GetComponent<AudioListener>());
                    //Destroy(camera);
                    camera.enabled = false;

                    foreach (MeshRenderer meshRenderer in camera.GetComponentsInChildren<MeshRenderer>())
                    {
                        meshRenderer.enabled = true;
                    }
                }
            }
            else
            {
                Debug.Log($"Rig {OwnerClientId} is player rig");
            }
        }
    }
}
