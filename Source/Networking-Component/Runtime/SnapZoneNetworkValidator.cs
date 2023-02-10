using Unity.Netcode;
using UnityEngine;
using VRBuilder.BasicInteraction.Validation;

namespace VRBuilder.Networking
{
    [RequireComponent(typeof(NetworkObject))]
    public class SnapZoneNetworkValidator : Validator
    {
        NetworkObject networkObject;

        private void Awake()
        {
            networkObject = GetComponent<NetworkObject>();
        }

        public override bool Validate(GameObject obj)
        {
            NetworkObject targetNetworkObject = obj.GetComponent<NetworkObject>();

            return targetNetworkObject != null && targetNetworkObject.IsOwner;
            //return targetNetworkObject != null && targetNetworkObject.OwnerClientId == networkObject.OwnerClientId;
        }
    }
}