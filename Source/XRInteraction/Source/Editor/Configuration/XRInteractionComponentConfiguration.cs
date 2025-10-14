using System.Collections.Generic;
using VRBuilder.Core.Editor.Configuration;

namespace VRBuilder.XRInteraction.Editor.Configuration
{
    /// <summary>
    /// Configuration for the default XR interaction component.
    /// </summary>
    public class XRInteractionComponentConfiguration : IInteractionComponentConfiguration
    {
        public static readonly string UseHandTrackingKey = "use-hand-tracking";
        private const string ControllerRigPrefab = "VRB_XR_Setup";
        private const string HandTrackingRigPrefab = "VRB_XR_Setup_Hands";

        /// <inheritdoc/>
        public string DisplayName => "XRI Integration";

        /// <inheritdoc/>
        public bool IsXRInteractionComponent => true;

        /// <inheritdoc/>
        public Dictionary<string, ConfigurationSetting> CustomSettingDefinitions
        {
            get
            {
                Dictionary<string, ConfigurationSetting> customParams = new Dictionary<string, ConfigurationSetting>();
                customParams.Add(UseHandTrackingKey, new ConfigurationSetting("Use hand tracking", typeof(bool), "If enabled, a rig supporting hand tracking will be added to the scene.", IsHandTrackingDisabled, HandTrackingChangedCallback));
                return customParams;
            }
        }

        /// <inheritdoc/>
        public string GetRigResourcesPath(Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey(UseHandTrackingKey) || parameters[UseHandTrackingKey] == null || (bool)parameters[UseHandTrackingKey] == false)
            {
                return ControllerRigPrefab;
            }
            else
            {
                return HandTrackingRigPrefab;
            }
        }
    }
}