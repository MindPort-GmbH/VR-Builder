// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.Menu
{
    internal static class ResourceMenuEntry
    {
        /// <summary>
        /// Allows opening the URL to VR Builder Resources.
        /// </summary>
        [MenuItem("Tools/VR Builder/Help/Tutorials and Resources", false, 80)]
        private static void OpenDocumentation()
        {
            Application.OpenURL("https://www.mindport.co/resources?utm_source=unity_editor&utm_medium=cpc&utm_campaign=from_unity&utm_id=from_unity");
        }
    }
}
