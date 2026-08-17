using System;
using UnityEngine;

namespace VRBuilder.Core.Configuration
{
    public class RuntimeConfiguration: ScriptableObject, IRuntimeConfiguration
    {
        /// <summary>
        /// Raised when the selected process changes. The string argument is the new streaming-assets path of the selected process.
        /// </summary>
        public event Action<string> SelectedProcessChanged;

        [SerializeField]
        private string selectedProcess;

        [SerializeField]
        private string manifestFileName = "ProcessManifest";

        public string SelectedProcess
        {
            get => selectedProcess;
            set
            {
                if (string.Equals(selectedProcess, value, StringComparison.Ordinal))
                {
                    return;
                }

                selectedProcess = value;
                SelectedProcessChanged?.Invoke(selectedProcess);
            }
        }

        public string ManifestFileName
        {
            get => manifestFileName;
            set => manifestFileName = value;
        }
    }
}