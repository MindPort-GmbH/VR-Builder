namespace VRBuilder.PackageManager.Editor
{
    /// <summary>
    /// Adds Unity's Post-Processing package as a dependency.
    /// </summary>
    public class PostProcessingPackageEnabler : Dependency
    {
        /// <inheritdoc/>
        public override string Package { get; } = "com.unity.postprocessing";

        /// <inheritdoc/>
        public override int Priority { get; } = 10;

        /// <inheritdoc/>
        protected override LayerDependency[] LayerDependencies { get; } = { new LayerDependency("Post-Processing") };
    }
}
