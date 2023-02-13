using System;
using Unity.Netcode;
using UnityEngine;
using VRBuilder.Core.Properties;
using VRBuilder.XRInteraction.Properties;

namespace VRBuilder.Networking
{
    [RequireComponent(typeof(TouchableProperty))]
    [RequireComponent(typeof(NetworkObject))]
    public class TouchablePropertyNetwork : LockablePropertyNetwork
    {
        protected TouchableProperty touchableProperty;
        protected override LockableProperty lockableProperty => touchableProperty;

        private void Awake()
        {
            touchableProperty = GetComponent<TouchableProperty>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                touchableProperty.Touched += OnTouched;
                touchableProperty.Untouched += OnUntouched;
            }

            base.OnNetworkSpawn();
        }

        private void OnTouched(object sender, EventArgs e)
        {
            SetTouchedServerRpc(true);
        }

        private void OnUntouched(object sender, EventArgs e)
        {
            SetTouchedServerRpc(false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetTouchedServerRpc(bool isTouched)
        { 
            touchableProperty.ForceSetTouched(isTouched);
        }
    }
}