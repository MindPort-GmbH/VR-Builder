using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRBuilder.Networking
{
    [RequireComponent(typeof(XRBaseInteractor))]
    public class NetworkInteractor : NetworkBehaviour
    {
        private XRBaseInteractor interactor;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();


            interactor = GetComponent<XRBaseInteractor>();

            if (IsOwner == false) 
            {
                return;
            }


            interactor.selectEntered.AddListener(OnSelectEntered);
            interactor.selectExited.AddListener(OnSelectExited);
        }

        private void OnSelectExited(SelectExitEventArgs arg0)
        {
            Debug.Log("Select exited");
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (IsOwner && IsClient)
            {
                NetworkObject selectedObject = args.interactableObject.transform.GetComponent<NetworkObject>();

                if (selectedObject != null && selectedObject.OwnerClientId != OwnerClientId)
                {
                    RequestOwnershipServerRpc(OwnerClientId, selectedObject);
                }
            }
        }        

        [ServerRpc]
        public void RequestOwnershipServerRpc(ulong ownerID, NetworkObjectReference selectedObjectReference)
        {
            NetworkObject selectedObject = null;
            if (selectedObjectReference.TryGet(out selectedObject))
            {
                selectedObject.ChangeOwnership(ownerID);
            }
        }
    }
}