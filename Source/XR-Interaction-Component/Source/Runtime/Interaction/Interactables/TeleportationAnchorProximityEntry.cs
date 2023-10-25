using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VRBuilder.Core.Properties;

namespace VRBuilder.XRInteraction
{
    public class TeleportationAnchorProximityEntry : MonoBehaviour
    {
        private TeleportationAnchor teleportationAnchor;

        private void Start()
        {
            teleportationAnchor = GetComponentInParent<TeleportationAnchor>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!teleportationAnchor.enabled)
                return;

            Transform objectRoot = other.transform.root;
            UserSceneObject userSceneObject = objectRoot.GetComponentInChildren<UserSceneObject>();
            if (userSceneObject != null)
            {
                teleportationAnchor.teleporting.Invoke(new TeleportingEventArgs());
            }
        }
    }
}
