namespace VRBuilder.Editor.Networking
{
    internal class NetworkManagerSceneSetup : SceneSetup
    {
        public static readonly string prefabName = "NETWORK_MANAGER";

        /// <inheritdoc/>
        public override void Setup()
        {
            SetupPrefab(prefabName);
        }

    }
}
