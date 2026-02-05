// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.Menu
{
    internal static class APIMenuEntry
    {
        /// <summary>
        /// Allows opening the URL to Creator Documentation.
        /// </summary>
        [MenuItem("Tools/VR Builder/Help/API Documentation", false, 79)]
        private static void OpenDocumentation()
        {
            Application.OpenURL("https://mindport-gmbh.github.io/VR-Builder-Documentation/api/VRBuilder.BasicInteraction.Behaviors.html?utm_source=unityeditor&utm_medium=unityeditor&utm_campaign=from_unity&utm_id=from_unity");
        }
    }
}
