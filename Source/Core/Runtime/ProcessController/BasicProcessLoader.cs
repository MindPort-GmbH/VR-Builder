using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;

namespace VRBuilder.ProcessController
{
    /// <summary>
    /// Loads and starts the process currently selected in the 'PROCESS_CONFIGURATION' gameObject.
    /// </summary>
    public class BasicProcessLoader : MonoBehaviour, IConfigurableProcessController
    {
        /// <inheritdoc />
        public bool AutoStartProcess { get; set; }

        private void Start()
        {
            StartCoroutine(StartProcess());
        }

        private IEnumerator StartProcess()
        {
            ProcessContainer[] processContainers = GameObject.FindObjectsByType<ProcessContainer>(FindObjectsSortMode.None);
            List<ProcessRunner.ProcessRunnerInstance> processRunners = new List<ProcessRunner.ProcessRunnerInstance>();

            foreach (ProcessContainer processContainer in processContainers)
            {
                // Load process from a file.
                string processPath = processContainer.GetSelectedProcess();

                // Try to load the in the PROCESS_CONFIGURATION selected process.

                Task<IProcess> loadProcess = processContainer.Configuration.LoadProcess(processPath);
                while (!loadProcess.IsCompleted)
                {
                    yield return null;
                }

                processRunners.Add(ProcessRunner.Initialize(loadProcess.Result));
            }

            // Runs the process.
            if(AutoStartProcess)
            {
                processRunners.ForEach(processRunner => processRunner.Execute());
            }
        }
    }
}
