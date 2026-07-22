using System;
using UnityEngine;
using UnityEngine.UI;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.User;

namespace VRBuilder.ProcessController
{
    /// <summary>
    /// Puts the parent GameObject to the same position and rotation of the user camera.
    /// </summary>
    public class AttachToUserView : MonoBehaviour
    {
        [Tooltip("The font used in the spectator view.")]
        [SerializeField]
        protected Font font;

        [Tooltip("Size of the font used")]
        [SerializeField]
        protected int fontSize = 30;

        private GameObject user;

        protected void Start()
        {
            SetFont();
        }

        protected void LateUpdate()
        {
            UpdateCameraPositionAndRotation();
        }

        private void UpdateCameraPositionAndRotation()
        {
            if (user == null)
            {
                var userObj = ServiceRegistry.Get<IUserService>()?.User as UserSceneObject;
                if (userObj == null) return;
                user = userObj.gameObject;
            }


            transform.SetPositionAndRotation(user.transform.position, user.transform.rotation);
        }

        private void SetFont()
        {
            foreach (Text text in GetComponentsInChildren<Text>(true))
            {
                text.font = font;
                text.fontSize = fontSize;
            }
        }
    }
}
