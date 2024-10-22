using VRBuilder.Editor.XRInteraction;
using VRBuilder.XRInteraction;

namespace VRBuilder.Editor.UI
{
    /// <summary>
    /// Settings section for <see cref="SnapZoneSettings"/>.
    /// </summary>
    public class SnapZoneSettingsSection : ComponentSettingsSection<SnapZone, SnapZoneSettings>
    {
        /// <inheritdoc/>        
        public override string Title => "Snap Zones";

        /// <inheritdoc/>        
        public override string Description => "These settings help you to configure Snap Zones within your scenes. You can define colors and other values that will be set to Snap Zones created by clicking the 'Create Snap Zone' button of a Snappable Property.";

        /// <inheritdoc/>        
        public override int Priority => 1000;
    }
}