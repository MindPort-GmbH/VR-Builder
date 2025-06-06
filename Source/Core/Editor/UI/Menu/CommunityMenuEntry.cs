// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.Menu
{
    internal static class CommunityMenuEntry
    {
        /// <summary>
        /// Allows to open the URL to the MindPort community.
        /// </summary>
        [MenuItem("Tools/VR Builder/MindPort Community", false, 128)]
        private static void OpenCommunityPage()
        {
            Application.OpenURL("http://community.mindport.co");
        }
    }
}
