// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using UnityEditor;
using UnityEngine;

namespace VRBuilder.Editor.BuilderMenu
{
    internal static class SupportMenuEntry
    {
        /// <summary>
        /// Allows to open the URL to MindPort's Jira Servicedesk.
        /// </summary>
        [MenuItem("Tools/VR Builder/Support", false, 128)]
        private static void OpenSupportPage()
        {
            Application.OpenURL("http://support.mindport.co");
        }
    }
}
