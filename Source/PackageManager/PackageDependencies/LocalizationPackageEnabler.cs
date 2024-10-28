namespace VRBuilder.PackageManager.Editor
{
    /// <summary>
    /// Adds Unity's Localizaion System package as a dependency.
    /// </summary>
    public class LocalizationPackageEnabler : Dependency
    {
        /// <inheritdoc/>
        public override string Package { get; } = "com.unity.localization";

        /// <inheritdoc/>
        public override int Priority { get; } = 4;
    }
}
