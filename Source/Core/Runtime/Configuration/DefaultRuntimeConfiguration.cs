// Copyright (c) 2013-2019 Innoactive GmbH
// Modifications copyright (c) 2021-2026 MindPort GmbH
// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Base class for your runtime process configuration. Extend it to create your own.
    /// </summary>
    [Obsolete("Use RuntimeConfiguration. Look at RuntimeConfigurationSetup")]
    public class DefaultRuntimeConfiguration : SettingsObject<DefaultRuntimeConfiguration>, IRuntimeConfiguration
    {
        /// <summary>
        /// Process name which is selected.
        /// </summary>
        /// <remarks>
        /// This field is filled by <see cref="RuntimeConfiguratorEditor"/>
        /// </remarks>
        [SerializeField]
        private string selectedProcessStreamingAssetsPath = "";

        [SerializeField]
        private string selectedProcess;

        [SerializeField]
        private string manifestFileName = "ProcessManifest";

        public string SelectedProcessStreamingAssetsPath
        {
            get => selectedProcessStreamingAssetsPath;
            set => selectedProcessStreamingAssetsPath = value;
        }

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
