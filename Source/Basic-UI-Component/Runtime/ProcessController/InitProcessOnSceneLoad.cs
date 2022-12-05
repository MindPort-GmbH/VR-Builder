using System;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using UnityEngine;

namespace VRBuilder.UX
{
    /// <summary>
    /// Initializes the <see cref="ProcessRunner"/> with the current selected process on scene start.
    /// </summary>
    public class InitProcessOnSceneLoad : MonoBehaviour
    {
        private void OnEnable()
        {
            InitProcess();
        }

        private void InitProcess()
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
        }
    }
}