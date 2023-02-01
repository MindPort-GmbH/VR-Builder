using Unity.Netcode;
using UnityEngine;

namespace VRBuilder.Networking
{
    public class PlayerCameraEnabler : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            foreach (Camera camera in GetComponentsInChildren<Camera>())
            {
                camera.enabled = IsOwner;
            }
        }
    }
}
