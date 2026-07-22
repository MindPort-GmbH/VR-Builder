using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.User;

namespace VRBuilder.UI.Console
{
    /// <summary>
    /// Places the console in front of the user. Triggered once every time the console is shown,
    /// the console does not follow the user afterwards.
    /// </summary>
    public class VRBConsolePlacer : MonoBehaviour
    {
        [SerializeField]
        private float distanceFromUser = 1.75f;

        [SerializeField]
        private float heightOffset = -0.4f;

        /// <summary>
        /// Positions the console in front of the user at eye height and rotates it to face them.
        /// </summary>
        public void PlaceInFrontOfUser()
        {
            Transform head = GetUserHead();

            if (head == null)
            {
                return;
            }

            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);

            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.ProjectOnPlane(head.up, Vector3.up);
            }

            forward.Normalize();

            Vector3 position = head.position + forward * distanceFromUser + Vector3.up * heightOffset;
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, Vector3.up));
        }

        private Transform GetUserHead()
        {
            var userObj = ServiceRegistry.Get<IUserService>()?.User as UserSceneObject;
            if (userObj != null)
            {
                Camera cam = userObj.GetComponentInChildren<Camera>();
                if (cam != null)
                    return cam.transform;
            }

            return Camera.main != null ? Camera.main.transform : null;
        }
    }
}
