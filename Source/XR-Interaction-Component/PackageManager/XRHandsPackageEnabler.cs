namespace VRBuilder.Editor.PackageManager.XRInteraction
{
    /// <summary>
    /// Adds Unity's XR Hands package as a dependency and sets specified symbol for script compilation.
    /// </summary>
    public class XRHandsPackageEnabler : Dependency
    {
        /// <inheritdoc/>
        public override string Package { get; } = "com.unity.xr.hands";

        /// <inheritdoc/>
        public override string Version { get; set; } = "1.3.0";

        /// <inheritdoc/>
        public override string[] Samples { get; } = { "HandVisualizer" };

        /// <inheritdoc/>
        //public override int Priority { get; } = 4;
    }
}