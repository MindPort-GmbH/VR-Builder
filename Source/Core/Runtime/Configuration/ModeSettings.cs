using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Settings object of the mode settings.
    /// </summary>
    public class ModeSettings : SettingsObject<ModeSettings>, IModeConfiguration
    {
        /// <summary>
        /// Name of the default service.
        /// </summary>
        public string ServiceTypeName => typeof(ModeService).FullName;
    }
}
