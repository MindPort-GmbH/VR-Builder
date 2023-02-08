using System;
using Unity.Netcode;
using UnityEngine;

namespace VRBuilder.Core.ProcessUtils
{
    [RequireComponent(typeof(ConfettiMachine))]
    public class ConfettiMachineNetwork : NetworkBehaviour
    {
        ConfettiMachine confettiMachine;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            confettiMachine= GetComponent<ConfettiMachine>();

            if(IsServer)
            {
                Debug.Log("Registered");
                confettiMachine.Activated += OnActivated;
            }
        }

        private void OnActivated(object sender, EventArgs args)
        {
            Debug.Log("Sending RPC");
            //RemoteActivateClientRpc(confettiMachine.GetComponent<ParticleSystem>().shape.radius, confettiMachine.EmissionDuration);
            RemoteActivateClientRpc(1f, confettiMachine.EmissionDuration);
        }

        [ClientRpc]
        public void RemoteActivateClientRpc(float radius, float duration)
        {
            if(IsHost)
            {
                return;
            }

            Debug.Log("Received RPC");
            confettiMachine.Activate(radius, duration);
        }
    }
}