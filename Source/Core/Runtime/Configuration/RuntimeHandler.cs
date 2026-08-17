// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using UnityEngine;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Configurator to set the process runtime configuration which is used by a process during its execution.
    /// There has to be one and only one process runtime configurator game object per scene.
    /// </summary>
    public sealed class RuntimeHandler : MonoBehaviour, IRuntimeHandler
    {
        /// <summary>
        /// Raised when the selected process changes. The string argument is the new streaming-assets path of the selected process.
        /// </summary>
        public event Action<string> SelectedProcessChanged;

        [SerializeField]
        private IRuntimeConfiguration runtimeConfiguration;

        private bool isSubscribedToConfiguration;

        public IRuntimeConfiguration RuntimeConfiguration
        {
            get
            {
                runtimeConfiguration ??= ScriptableObject.CreateInstance<RuntimeConfiguration>();

                EnsureConfigurationSubscription();

                return runtimeConfiguration;
            }
            set
            {
                if (ReferenceEquals(runtimeConfiguration, value))
                {
                    return;
                }

                UnsubscribeFromConfiguration();

                string previousSelection = runtimeConfiguration?.SelectedProcess;
                runtimeConfiguration = value;

                SubscribeToConfiguration();

                string currentSelection = runtimeConfiguration?.SelectedProcess;
                if (previousSelection != currentSelection)
                {
                    SelectedProcessChanged?.Invoke(currentSelection);
                }
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromConfiguration();
        }

        private void SubscribeToConfiguration()
        {
            if (runtimeConfiguration is RuntimeConfiguration configuration)
            {
                configuration.SelectedProcessChanged += OnConfigurationSelectedProcessChanged;
                isSubscribedToConfiguration = true;
            }
        }

        private void UnsubscribeFromConfiguration()
        {
            if (runtimeConfiguration is RuntimeConfiguration configuration && isSubscribedToConfiguration)
            {
                configuration.SelectedProcessChanged -= OnConfigurationSelectedProcessChanged;
                isSubscribedToConfiguration = false;
            }
        }

        private void EnsureConfigurationSubscription()
        {
            if (runtimeConfiguration is RuntimeConfiguration configuration && isSubscribedToConfiguration == false)
            {
                configuration.SelectedProcessChanged += OnConfigurationSelectedProcessChanged;
                isSubscribedToConfiguration = true;
            }
        }

        private void OnConfigurationSelectedProcessChanged(string selectedProcess)
        {
            SelectedProcessChanged?.Invoke(selectedProcess);
        }
    }
}
