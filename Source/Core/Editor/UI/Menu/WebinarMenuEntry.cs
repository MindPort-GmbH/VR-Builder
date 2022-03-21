// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using UnityEditor;
using UnityEngine;

namespace VRBuilder.Editor.BuilderMenu
{
    internal static class WebinarMenuEntry
    {
        /// <summary>
        /// Allows to open the URL to webinar.
        /// </summary>
        [MenuItem("Tools/VR Builder/Innoactive Help/Webinar", false, 80)]
        private static void OpenWebinar()
        {
            Application.OpenURL("https://vimeo.com/417328541/93a752e72c");
        }
    }
}
