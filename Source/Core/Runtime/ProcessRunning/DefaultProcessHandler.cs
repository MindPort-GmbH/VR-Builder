// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.ProcessRunning;
using VRBuilder.Core.Runtime.Registry;

namespace VRBuilder.Unity.ProcessRunning
{
    /// <summary>
    /// Loads and starts the process currently selected in the 'PROCESS_CONFIGURATION' gameObject.
    /// </summary>
    public class DefaultProcessHandler : MonoBehaviour, IConfigurableProcessHandler
    {
        private IProcessRunner? processRunner;

        [SerializeField]
        private bool autoStartProcess;

        public IProcessRunner ProcessRunner
        {
            get => processRunner ??= ServiceRegistry.Get<IProcessRunner>();
            set => processRunner = value;
        }

        public bool AutoStartProcess
        {
            get => autoStartProcess;
            set => autoStartProcess = value;
        }

        public async void LoadAndStartProcess()
        {
            //TODO handle multiple process runner
            try
            {
                // Try to load the selected process of the runtime service.
                IProcess process = await ServiceRegistry.Get<IRuntimeService>().LoadProcess();

                // Initializes the process.
                ProcessRunner.Initialize(process);
                SceneManager.sceneUnloaded -= OnSceneUnloaded;
                SceneManager.sceneUnloaded += OnSceneUnloaded;

                // Runs the process
                if (AutoStartProcess)
                {
                    ProcessRunner.Start();
                }
            }
            catch (Exception e)
            {
                ForwardingLogger.LogError($"Error while starting process: {e.Message}");
            }
        }

        public void StartProcess()
        {
            //TODO handle multiple process runner
            ProcessRunner.Start();
        }

        public void StopProcess()
        {
            //TODO handle multiple process runner
            ProcessRunner.Stop();
        }

        public void Initialize(IProcess process)
        {
            //TODO handle multiple process runner
            ProcessRunner.Initialize(process);
        }

        private void OnSceneUnloaded(Scene scene)
        {
            ProcessRunner.OnSceneUnloaded(scene.name);
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        public void Update()
        {
            if (ProcessRunner is { IsRunning: true })
                ProcessRunner.Update();
        }

        private void OnDisable()
        {
            ProcessRunner.Stop();
        }
    }
}
