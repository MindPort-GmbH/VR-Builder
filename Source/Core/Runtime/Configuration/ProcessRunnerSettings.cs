using UnityEngine;
using VRBuilder.Core.ProcessRunning;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Configuration
{
    [CreateAssetMenu(fileName = "ProcessRunnerSettings", menuName = "VR Builder/Process Runner Settings", order = 1)]
    public class ProcessRunnerSettings : SettingsObject<ProcessRunnerSettings>, IProcessRunnerConfiguration
    {
        [SerializeField]
        private bool resetEventsOnSceneUnload = true;

        public bool ResetEventsOnSceneUnload => resetEventsOnSceneUnload;
    }
}