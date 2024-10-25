namespace VRBuilder.PackageManager.Editor
{
    /// <summary>
    /// A layer to be created by a <see cref="Dependency"/>.
    /// </summary>
    public struct LayerDependency
    {
        /// <summary>
        /// The name of the layer to be created.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The preferred position in the array for the layer.
        /// </summary>
        public readonly int PreferredPosition;

        public LayerDependency(string name, int preferredPosition = -1)
        {
            Name = name;
            PreferredPosition = preferredPosition;
        }
    }
}