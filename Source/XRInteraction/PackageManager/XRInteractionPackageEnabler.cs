namespace VRBuilder.PackageManager.Editor.XRInteraction
{
    /// <summary>
    /// Adds Unity's XR-Interaction-Toolkit package as a dependency and sets specified symbol for script compilation.
    /// </summary>
    public class XRInteractionPackageEnabler : Dependency
    {
        /// <inheritdoc/>
        public override string Package { get; } = "com.unity.xr.interaction.toolkit";

        /// <inheritdoc/>
        public override string Version { get; set; } = "3.1.2";

        /// <inheritdoc/>
        public override int Priority { get; } = 4;

        /// <inheritdoc/>
        protected override LayerDependency[] LayerDependencies { get; } = { new LayerDependency("Teleport", 31) };
    }
}