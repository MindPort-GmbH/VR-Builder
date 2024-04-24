using UnityEngine;

using VRBuilder.Core.Properties;

namespace VRBuilder.XRInteraction
{
    /// <summary>
    /// This adds the possibility to move the user into a <seealso cref="TeleportationAnchor"/> and trigger the teleport event without teleporting.
    /// It will not change the users position or rotation set in the <seealso cref="TeleportationAnchor"/>.
    /// </summary>
    public class TeleportationAnchorProximityEntry : MonoBehaviour
    {
        private UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor teleportationAnchor;

        private void Start()
        {
            teleportationAnchor = GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!teleportationAnchor.enabled)
                return;

            Transform objectRoot = other.transform.root;
            UserSceneObject userSceneObject = objectRoot.GetComponentInChildren<UserSceneObject>();
            if (userSceneObject != null)
            {
                teleportationAnchor.teleporting.Invoke(new UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportingEventArgs());
            }
        }
    }
}
