using VRBuilder.Core.Configuration;

namespace VRBuilder.XRInteraction.Configuration
{
    /// <summary>
    /// Configuration for the default XR interaction component.
    /// </summary>
    public class XRInteractionComponentConfiguration : IInteractionComponentConfiguration
    {
        /// <inheritdoc/>
        public string DisplayName => "XRI Integration";

        /// <inheritdoc/>
        public bool IsXRInteractionComponent => true;

        /// <inheritdoc/>
        public string DefaultRigPrefab => "VRB_XR_Setup";

        /// <inheritdoc/>
        public bool IsHandTrackingSupported => throw new System.NotImplementedException();
    }
}