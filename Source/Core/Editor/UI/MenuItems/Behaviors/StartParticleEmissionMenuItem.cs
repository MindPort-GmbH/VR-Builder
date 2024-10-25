using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Core.Editor.UI.MenuItems.Behaviors
{
    /// <inheritdoc />
    public class StartParticleEmissionMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Environment/Start Particle Emission";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            return new ControlParticleEmissionBehavior(true);
        }
    }
}
