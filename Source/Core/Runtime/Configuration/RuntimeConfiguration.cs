// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Configuration of the runtime to hold the current selected process and related metadata.
    /// At editor time this can be used for setting the process in the scene as well as saving it.
    /// At runtime the process should also be selectable but should be readonly to not have side effects.
    /// </summary>
    public class RuntimeConfiguration: ScriptableObject, IRuntimeConfiguration
    {
        [SerializeField]
        private string selectedProcess;

        [SerializeField]
        private string manifestFileName = "ProcessManifest";

        public string SelectedProcess
        {
            get => selectedProcess;
            set => selectedProcess = value;
        }

        public string ManifestFileName
        {
            get => manifestFileName;
            set => manifestFileName = value;
        }
    }
}
