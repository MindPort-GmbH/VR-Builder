using System;
using System.Collections.Generic;
using VRBuilder.Core.Configuration;

namespace VRBuilder.XRInteraction.Configuration
{
    /// <summary>
    /// Configuration for the default XR interaction component.
    /// </summary>
    public class XRInteractionComponentConfiguration : IInteractionComponentConfiguration
    {
        private const string UseHandTrackingKey = "use-hand-tracking";

        /// <inheritdoc/>
        public string DisplayName => "XRI Integration";

        /// <inheritdoc/>
        public bool IsXRInteractionComponent => true;

        /// <inheritdoc/>
        [Obsolete("Use GetRigResourcesPath instead")]
        public string DefaultRigPrefab => "VRB_XR_Setup";

        /// <inheritdoc/>
        public bool IsHandTrackingSupported => true;

        public Dictionary<string, Parameter> ParametersTemplate
        {
            get
            {
                Dictionary<string, Parameter> customParams = new Dictionary<string, Parameter>();
                customParams.Add(UseHandTrackingKey, new Parameter("Use hand tracking", typeof(bool), "Tooltip"));
                return customParams;
            }
        }

        /// <inheritdoc/>
        public string GetRigResourcesPath(Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey(UseHandTrackingKey) || parameters[UseHandTrackingKey] == null || (bool)parameters[UseHandTrackingKey] == false)
            {
                return "VRB_XR_Setup";
            }
            else
            {
                return "VRB_XR_Setup_Hands";
            }
        }
    }
}