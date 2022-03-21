// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using UnityEditor;

namespace VRBuilder.Editor.BuilderMenu
{
    internal static class SetupSceneEntry
    {
        /// <summary>
        /// Setup the current unity scene to be a functioning process scene.
        /// </summary>
        [MenuItem("Tools/VR Builder/Setup Scene for VR Process", false, 2)]
        public static void SetupScene()
        {
            ProcessSceneSetup.Run();
        }
    }
}
