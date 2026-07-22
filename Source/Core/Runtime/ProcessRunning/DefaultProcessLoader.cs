// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.ProcessRunning;
using VRBuilder.Core.Registry;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.ProcessController;

namespace VRBuilder.Unity.ProcessRunning
{
    /// <summary>
    /// Loads and starts the process currently selected in the 'PROCESS_CONFIGURATION' gameObject.
    /// </summary>
    public class DefaultProcessLoader : MonoBehaviour, IConfigurableProcessController
    {
        private IProcessRunner processRunner;

        [SerializeField]
        private bool autoStartProcess;

        public IProcessRunner ProcessRunner
        {
            get => processRunner ?? ServiceRegistry.Get<IProcessRunner>();
            set => processRunner = value;
        }

        /// <inheritdoc />
        public bool AutoStartProcess
        {
            get => autoStartProcess;
            set => autoStartProcess = value;
        }

        protected virtual void Start()
        {
            StartCoroutine(StartProcess());
        }

        protected IEnumerator StartProcess()
        {
            // Load process from a file.
            string processPath = RuntimeConfigurator.Instance.GetSelectedProcess();

            // Try to load the in the PROCESS_CONFIGURATION selected process.

            Task<IProcess> loadProcess = RuntimeConfigurator.Configuration.LoadProcess(processPath);
            while (!loadProcess.IsCompleted)
            {
                yield return null;
            }

            IProcess process = loadProcess.Result;

            // Initializes the process.
            ProcessRunner.Initialize(process);
            SceneManager.sceneUnloaded -= OnSceneUnloaded; // guard
            SceneManager.sceneUnloaded += OnSceneUnloaded;


            // Runs the process.
            if (AutoStartProcess)
            {
                ProcessRunner.Start();
            }
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