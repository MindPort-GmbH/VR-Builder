using System;
using Unity.Netcode;
using UnityEngine;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.XRInteraction.Properties;

namespace VRBuilder.Networking
{
    [RequireComponent(typeof(SnappableProperty))]
    public class SnappablePropertyNetwork : NetworkBehaviour
    {
        protected SnappableProperty snappableProperty;
        protected NetworkVariable<bool> isLocked = new NetworkVariable<bool>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            snappableProperty = GetComponent<SnappableProperty>();

            if (IsServer)
            {
                isLocked.Value = snappableProperty.IsLocked;

                snappableProperty.Locked += OnLocked;
                snappableProperty.Unlocked += OnUnlocked;
            }

            if (IsClient)
            {
                snappableProperty.Snapped += OnSnapped;
                snappableProperty.Unsnapped += OnUnsnapped;

                snappableProperty.SetLocked(isLocked.Value);
                isLocked.OnValueChanged += OnLockedValueChanged;
            }
        }

        private void OnSnapped(object sender, EventArgs e)
        {
            NetworkObject snapZoneObject = snappableProperty.SnappedZone.SceneObject.GameObject.GetComponent<NetworkObject>();

            if(snapZoneObject != null)
            {
                SetSnappedServerRPC(snapZoneObject, true);
            }            
        }

        private void OnUnsnapped(object sender, EventArgs e)
        {            
            SetSnappedServerRPC(new NetworkObject(), false);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetSnappedServerRPC(NetworkObjectReference snapZoneObjectReference, bool isSnapped)
        {
            NetworkObject snapZoneObject;
            ISnapZoneProperty snapZoneProperty = null;
            if(snapZoneObjectReference.TryGet(out snapZoneObject))
            {
                snapZoneProperty = snapZoneObject.GetComponent<SnapZoneProperty>();
            }

            if(isSnapped && snapZoneProperty != null)
            {
                snappableProperty.SnappedZone = snapZoneProperty;
            }
            else
            {
                snappableProperty.SnappedZone = null;
            }
        }

        private void OnLockedValueChanged(bool previousValue, bool newValue)
        {
            if (previousValue == newValue)
            {
                return;
            }

            Debug.Log("Value changed to " + newValue);

            if (newValue)
            {
                snappableProperty.SetLocked(true);
            }
            else
            {
                snappableProperty.SetLocked(false);
            }
        }

        private void OnLocked(object sender, LockStateChangedEventArgs e)
        {
            if (IsServer)
            {
                isLocked.Value = true;
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
    }
}