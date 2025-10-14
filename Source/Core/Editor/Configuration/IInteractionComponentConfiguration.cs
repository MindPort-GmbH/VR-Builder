using System.Collections.Generic;

namespace VRBuilder.Core.Editor.Configuration
{
    /// <summary>
    /// Configuration for an interaction component.
    /// </summary>
    public interface IInteractionComponentConfiguration
    {
        /// <summary>
        /// Display name of the interaction component.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// True if the interaction component is meant to work with XR.
        /// </summary>
        bool IsXRInteractionComponent { get; }

        /// <summary>
        /// Returns the Resources path of the required rig.
        /// </summary>
        /// <param name="parameters">Custom parameters for the current configuration.</param>
        /// <returns>The Resources path of the rig.</returns>
        string GetRigResourcesPath(Dictionary<string, object> parameters);

        /// <summary>
        /// Defines the custom settings implemented by this configuration.
        /// </summary>
        Dictionary<string, ConfigurationSetting> CustomSettingDefinitions { get; }
    }
}
