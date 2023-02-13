using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using VRBuilder.Core.SceneObjects;
using VRBuilder.XRInteraction.Properties;

namespace VRBuilder.Networking
{
    [RequireComponent(typeof(GrabbableProperty))]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(ClientNetworkTransform))]
    [RequireComponent(typeof(NetworkRigidbody))]
    public class GrabbablePropertyNetwork : NetworkBehaviour
    {
        protected GrabbableProperty grabbableProperty;
        protected NetworkVariable<bool> isLocked = new NetworkVariable<bool>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            grabbableProperty = GetComponent<GrabbableProperty>();

            if (IsServer)
            {
                isLocked.Value = grabbableProperty.IsLocked;

                grabbableProperty.Locked += OnLocked;
                grabbableProperty.Unlocked += OnUnlocked;
            }

            if (IsClient)
            {
                grabbableProperty.Grabbed += OnGrabbed;
                grabbableProperty.Ungrabbed += OnUngrabbed;

                grabbableProperty.SetLocked(isLocked.Value);
                isLocked.OnValueChanged += OnLockedValueChanged;
            }
        }

        private void OnLockedValueChanged(bool previousValue, bool newValue)
        {
            if(previousValue == newValue)
            {
                return;
            }

            Debug.Log("Value changed to " + newValue);

            if(newValue)
            {
                grabbableProperty.SetLocked(true);
            }
            else
            {
                grabbableProperty.SetLocked(false);
            }
        }

        private void OnLocked(object sender, LockStateChangedEventArgs e)
        {
            if(IsServer)
            {
                isLocked.Value= true;
                Debug.Log("Updating to true");
            }
        }

        private void OnUnlocked(object sender, LockStateChangedEventArgs e)
        {
            if (IsServer)
            {
                isLocked.Value = false;
                Debug.Log("Updating to false");
            }
        }

        private void OnGrabbed(object sender, EventArgs e)
        {
            SetGrabbedServerRPC(true);
        }

        private void OnUngrabbed(object sender, EventArgs e)
        {
            SetGrabbedServerRPC(false);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetGrabbedServerRPC(bool isGrabbed)
        {
            grabbableProperty.ForceSetGrabbed(isGrabbed);
        }
    }
}