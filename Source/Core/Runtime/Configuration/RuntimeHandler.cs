// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH
// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Handler to set the process runtime configuration which is used by a process during its execution.
    /// There has to be one and only one process runtime configurator game object per scene.
    /// </summary>
    public sealed class RuntimeHandler : MonoBehaviour, IRuntimeHandler
    {
        [SerializeField]
        private IRuntimeConfiguration runtimeConfiguration;

        private event Action<string> selectedProcessChanged;

        public string SelectedProcess
        {
            get => runtimeConfiguration.SelectedProcess;
            set
            {
                if (runtimeConfiguration.SelectedProcess != value)
                {
                    runtimeConfiguration.SelectedProcess = value;
                    selectedProcessChanged?.Invoke(value);
                }
            }
        }

        public event Action<string> SelectedProcessChanged
        {
            add => selectedProcessChanged += value;
            remove => selectedProcessChanged -= value;
        }

        public IRuntimeConfiguration RuntimeConfiguration
        {
            get
            {
                runtimeConfiguration ??= ScriptableObject.CreateInstance<RuntimeConfiguration>();

                return runtimeConfiguration;
            }
            set
            {
                if (ReferenceEquals(runtimeConfiguration, value))
                {
                    return;
                }

                string previousSelectedProcess = runtimeConfiguration?.SelectedProcess;
                runtimeConfiguration = value;

                string currentSelectedProcess = runtimeConfiguration?.SelectedProcess;
                if (previousSelectedProcess != currentSelectedProcess)
                {
                    selectedProcessChanged?.Invoke(currentSelectedProcess);
                }
            }
        }
    }
}
