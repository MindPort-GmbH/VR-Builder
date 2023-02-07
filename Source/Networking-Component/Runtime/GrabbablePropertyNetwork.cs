using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
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

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            grabbableProperty = GetComponent<GrabbableProperty>();

            grabbableProperty.Grabbed += OnGrabbed;
            grabbableProperty.Ungrabbed += OnUngrabbed;
        }

        private void OnGrabbed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnUngrabbed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}