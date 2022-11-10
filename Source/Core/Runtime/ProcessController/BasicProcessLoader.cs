using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using System;

namespace VRBuilder.ProcessController
{
    /// <summary>
    /// Loads and starts the process currently selected in the 'PROCESS_CONFIGURATION' gameObject.
    /// </summary>
    public class BasicProcessLoader : MonoBehaviour
    {
       private void Start()
        {
            // Load process from a file.
            string processPath = RuntimeConfigurator.Instance.GetSelectedProcess();

            IProcess process;

            // Try to load the in the PROCESS_CONFIGURATION selected process.
            try
            {
                process = RuntimeConfigurator.Configuration.LoadProcess(processPath);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Error when loading process. {exception.GetType().Name}, {exception.Message}\n{exception.StackTrace}", RuntimeConfigurator.Instance.gameObject);
                return;
            }

            // Initializes the process. That will synthesize an audio for the instructions, too.
            ProcessRunner.Initialize(process);

            // Runs the process.
            ProcessRunner.Run();
        }
    }
}
