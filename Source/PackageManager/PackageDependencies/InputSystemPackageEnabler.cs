namespace VRBuilder.PackageManager.Editor
{
    /// <summary>
    /// Adds Unity's Input System package as a dependency.
    /// </summary>
    public class InputSystemPackageEnabler : Dependency
    {
        /// <inheritdoc/>
        public override string Package { get; } = "com.unity.inputsystem";

        /// <inheritdoc/>
        public override int Priority { get; } = 5;
    }
}
