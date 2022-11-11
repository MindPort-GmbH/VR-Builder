// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;

namespace VRBuilder.Editor
{
    /// <summary>
    /// Will setup a <see cref="RuntimeConfigurator"/> when none is existent in scene.
    /// </summary>
    internal class RuntimeConfigurationSetup : SceneSetup
    {
        public static readonly string ProcessConfiugrationName = "PROCESS_CONFIGURATION";
        /// <inheritdoc/>
        public override void Setup()
        {
            if (RuntimeConfigurator.Exists == false)
            {
                GameObject obj = new GameObject(ProcessConfiugrationName);
                obj.AddComponent<RuntimeConfigurator>();
                Selection.activeObject = obj;
            }
        }
    }
}
