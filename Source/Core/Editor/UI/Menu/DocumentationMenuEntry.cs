// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.Menu
{
    internal static class DocumentationMenuEntry
    {
        /// <summary>
        /// Allows to open the URL to Creator Documentation.
        /// </summary>
        [MenuItem("Tools/VR Builder/VR Builder Help/Manual", false, 80)]
        private static void OpenDocumentation()
        {
            Application.OpenURL("https://mindport-gmbh.github.io/VR-Builder-Documentation/index.html");
        }
    }
}
