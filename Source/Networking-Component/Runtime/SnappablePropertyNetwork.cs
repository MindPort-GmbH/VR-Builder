using System;
using System.Runtime.InteropServices.WindowsRuntime;
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
                snappableProperty.ObjectSnapped.AddListener(OnSnapped);
                snappableProperty.ObjectUnsnapped.AddListener(OnUnsnapped);

                snappableProperty.SetLocked(isLocked.Value);
                isLocked.OnValueChanged += OnLockedValueChanged;
            }
        }

        private void OnUnsnapped(SnappablePropertyEventArgs args)
        {
            Debug.Log($"Processing unsnap for {snappableProperty.SceneObject.UniqueName}.");
            NetworkObject snapZoneObject = args.SnappedZone.SceneObject.GameObject.GetComponent<NetworkObject>();

            if (snapZoneObject != null)
            {
                SetSnappedServerRpc(snapZoneObject, false);
            }
        }

        private void OnSnapped(SnappablePropertyEventArgs args)
        {
            NetworkObject snapZoneObject = args.SnappedZone.SceneObject.GameObject.GetComponent<NetworkObject>();

            if (snapZoneObject != null)
            {
                SetSnappedServerRpc(snapZoneObject, true);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetSnappedServerRpc(NetworkObjectReference snapZoneObjectReference, bool isSnapped)
        {
            NetworkObject snapZoneObject;
            ISnapZoneProperty snapZoneProperty = null;
            if(snapZoneObjectReference.TryGet(out snapZoneObject))
            {
                snapZoneProperty = snapZoneObject.GetComponent<SnapZoneProperty>();
            }

            if(snapZoneProperty == null)
            {
                Debug.LogError("Null snap zone property");
                return;
            }

            if(isSnapped) 
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