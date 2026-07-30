using UnityEngine;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Configuration
{
    public class RuntimeServiceConfiguration : SettingsObject<RuntimeServiceConfiguration>, IRuntimeServiceConfiguration
    {
        [SerializeField]
        private string selectedProcessStreamingAssetsPath;

        public string SelectedProcessStreamingAssetsPath
        {
            get => selectedProcessStreamingAssetsPath;
            set => selectedProcessStreamingAssetsPath = value;
        }
    }
}