// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using UnityEngine;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Used to identify the user within the scene.
    /// </summary>
    public class UserSceneObject : MonoBehaviour, IXRRigTransform
    {
        [SerializeField]
        private Transform head, leftHand, rightHand, rigBase;

        /// <summary>
        /// Returns the user's head transform.
        /// </summary>
        public Transform Head
        {
            get
            {
                if (head == null)
                {
                    head = GetComponentInChildren<Camera>().transform;
                    Debug.LogWarning("User head object is not referenced on User Scene Object component. The rig's camera will be used, if available.");
                }

                return head;
            }
        }

        /// <summary>
        /// Returns the user's left hand transform.
        /// </summary>
        public Transform LeftHand => leftHand;

        /// <summary>
        /// Returns the user's right hand transform.
        /// </summary>
        public Transform RightHand => rightHand;

        /// <summary>
        /// Returns the base of the rig.
        /// </summary>
        public Transform Base => rigBase;
    }
}
