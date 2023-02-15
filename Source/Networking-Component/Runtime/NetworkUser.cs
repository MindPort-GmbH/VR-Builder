using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using VRBuilder.XRInteraction;

namespace VRBuilder.Networking
{
    public class NetworkUser : NetworkBehaviour
    {
        [SerializeField]
        protected List<Material> userMaterials = new List<Material>();

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

            SetUserMaterial();
            PlaceOnSpawnPoint();
        }

        private void PlaceOnSpawnPoint()
        {
            UserSpawn[] spawnPoints = GameObject.FindObjectsByType<UserSpawn>(FindObjectsSortMode.None);

            spawnPoints.OrderBy(spawn => spawn.Id);

            if(spawnPoints != null && spawnPoints.Length > 0) 
            {
                int spawnPosition = (int)OwnerClientId % spawnPoints.Length;

                transform.position = spawnPoints[spawnPosition].transform.position;
            }
        }

        private void SetUserMaterial()
        {
            if (userMaterials.Count == 0) 
            {
                return;
            }

            int materialId = (int)OwnerClientId % userMaterials.Count;

            foreach (MeshRenderer meshRenderer in GetComponentsInChildren<MeshRenderer>(true))
            {
                meshRenderer.material = userMaterials[materialId];
            }

            foreach (SkinnedMeshRenderer skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                skinnedMeshRenderer.material = userMaterials[materialId];
            }
        }
    }
}
