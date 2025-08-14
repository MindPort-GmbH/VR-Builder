namespace VRBuilder.PackageManager.Editor.XRInteraction
{
    /// <summary>
    /// Adds Unity's XR-Interaction-Toolkit package as a dependency and sets specified symbol for script compilation.
    /// </summary>
    public class XRHandsPackageEnabler : Dependency
    {
        /// <inheritdoc/>
        public override string Package { get; } = "com.unity.xr.hands";

        /// <inheritdoc/>
        public override string Version { get; set; } = "1.5.1";

        /// <inheritdoc/>
        public override int Priority { get; } = 4;
    }
}