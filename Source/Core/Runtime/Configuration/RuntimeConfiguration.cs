using UnityEngine;

namespace VRBuilder.Core.Configuration
{
    public class RuntimeConfiguration: ScriptableObject, IRuntimeConfiguration
    {
        [SerializeField]
        private string selectedProcess;

        [SerializeField]
        private string manifestFileName;

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