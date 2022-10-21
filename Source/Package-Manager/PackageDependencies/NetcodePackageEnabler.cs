namespace VRBuilder.Editor.PackageManager
{
    public class NetcodePackageEnabler : Dependency
    {
        /// <inheritdoc/>
        public override string Package { get; } = "com.unity.netcode.gameobjects";

        /// <inheritdoc/>
        public override int Priority { get; } = 8;
    }
}