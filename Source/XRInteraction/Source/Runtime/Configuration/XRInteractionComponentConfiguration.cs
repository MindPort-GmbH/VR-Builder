using System.Collections.Generic;
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
        public bool IsHandTrackingSupported => true;

        public Dictionary<string, Parameter> CustomParams
        {
            get
            {
                Dictionary<string, Parameter> customParams = new Dictionary<string, Parameter>();
                customParams.Add("use-hand-tracking", new Parameter("Use hand tracking", typeof(bool), "Tooltip"));
                return customParams;
            }
        }

        //public Dictionary<string, Parameter> CustomParams => new Dictionary<string, Parameter>()
        //{
        //    new KeyValuePair<string, Parameter>("UseHandTracking", new Parameter("Use Hand Tracking", typeof(bool), "If enabled, the rig will use hand tracking instead of controllers if the platform supports it.")),
        //};

        /// <inheritdoc/>
        public string GetRigResourcesPath(Dictionary<string, object> parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}