// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using Core.Runtime.Utils;
using UnityEngine;
using VRBuilder.Core.User;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Used to identify the user within the scene. This is just a "Tag" now. place it on the base of your User and then use GetComponentInChildren etc. to get your actual Properties like Camera, etc.
    /// </summary>
    public class UserSceneObject : ProcessSceneObject, IUserSceneObject
    {
    }
}