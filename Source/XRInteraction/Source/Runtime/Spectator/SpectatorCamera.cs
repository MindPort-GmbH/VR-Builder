using System;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.User;

namespace VRBuilder.UI.Spectator
{
    /// <summary>
    /// Spectator camera which sets its viewpoint to the one of the user.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class SpectatorCamera : MonoBehaviour
    {
        private GameObject user;

        protected virtual void Start()
        {
            var userObj = UserLocator.Current?.User as UserSceneObject;
            if (userObj != null)
                user = userObj.GameObject();
        }

        protected virtual void Update()
        {
            UpdateCameraPositionAndRotation();
        }

        /// <summary>
        /// Sets the position and rotation of the spectator camera to the one of the user.
        /// </summary>
        protected virtual void UpdateCameraPositionAndRotation()
        {
            if (user == null)
            {
                Start();
            }

            transform.SetPositionAndRotation(user.transform.position, user.transform.rotation);
        }
    }
}