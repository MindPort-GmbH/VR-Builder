using VRBuilder.Editor.XRInteraction;
using VRBuilder.XRInteraction;

namespace VRBuilder.Editor.UI
{
    public class SnapZoneSettingsSection : ComponentSettingsSection<SnapZone, SnapZoneSettings>
    {
        public override string Title => "Snap Zones";

        public override string Description => "These settings help you to configure Snap Zones within your scenes. You can define colors and other values that will be set to Snap Zones created by clicking the 'Create Snap Zone' button of a Snappable Property.";

        public override int Priority => 1000;
    }
}