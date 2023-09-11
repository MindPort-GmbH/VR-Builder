// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using UnityEngine;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Used to identify the user within the scene.
    /// </summary>
    public class UserSceneObject : ProcessSceneObject
    {
        [SerializeField]
        private Transform head, leftHand, rightHand;

        /// <summary>
        /// Returns the user's head transform.
        /// </summary>
        public Transform Head => head;

        /// <summary>
        /// Returns the user's left hand transform.
        /// </summary>
        public Transform LeftHand => leftHand;

        /// <summary>
        /// Returns the user's right hand transform.
        /// </summary>
        public Transform RightHand => rightHand;

        protected new void Awake()
        {
            base.Awake();
            uniqueName = "User";
        }
    }
}
