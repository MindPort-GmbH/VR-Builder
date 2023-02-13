using System;
using Unity.Netcode;
using UnityEngine;
using VRBuilder.Core.Properties;
using VRBuilder.XRInteraction.Properties;

namespace VRBuilder.Networking
{
    [RequireComponent(typeof(TouchableProperty))]
    public class TouchablePropertyNetwork : LockablePropertyNetwork
    {
        protected TouchableProperty touchableProperty;
        protected override LockableProperty lockableProperty => touchableProperty;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            touchableProperty = GetComponent<TouchableProperty>();

            if (IsClient)
            {
                touchableProperty.Touched += OnTouched;
                touchableProperty.Untouched += OnUntouched;
            }
        }

        private void OnTouched(object sender, EventArgs e)
        {
            SetTouchedServerRpc(true);
        }

        private void OnUntouched(object sender, EventArgs e)
        {
            SetTouchedServerRpc(false);
        }

        [ServerRpc]
        public void SetTouchedServerRpc(bool isTouched)
        { 
            touchableProperty.ForceSetTouched(isTouched);
        }
    }
}