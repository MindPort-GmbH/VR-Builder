using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.XRInteraction.Interactables
{
    /// <summary>
    /// This adds the possibility to move the user into a <seealso cref="TeleportationAnchor"/> and trigger the teleport event without teleporting.
    /// It will not change the users position or rotation set in the <seealso cref="TeleportationAnchor"/>.
    /// </summary>
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
