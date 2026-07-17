using UnityEngine;
using VRBuilder.Core.ProcessRunning;
using VRBuilder.Core.Settings;
using VRBuilder.Unity.ProcessRunning;

namespace VRBuilder.Core.Configuration
{
    [CreateAssetMenu(fileName = "ProcessRunnerSettings", menuName = "VR Builder/Process Runner Settings", order = 1)]
    public class ProcessRunnerSettings : SettingsObject<ProcessRunnerSettings>, IProcessRunnerConfiguration
    {
        [SerializeField]
        private string runnerName = typeof(DefaultProcessRunner).FullName;

        [SerializeField]
        private bool resetEventsOnSceneUnload = true;

        public string RunnerName => runnerName;
        public bool ResetEventsOnSceneUnload => resetEventsOnSceneUnload;
    }
}