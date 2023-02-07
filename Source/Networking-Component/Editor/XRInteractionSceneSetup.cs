namespace VRBuilder.Editor.Networking
{
    internal class XRInteractionSceneSetup : SceneSetup
    {
        public static readonly string prefabName = "XR_INTERACTION";

        /// <inheritdoc/>
        public override void Setup()
        {
            SetupPrefab(prefabName);
        }

    }
}
