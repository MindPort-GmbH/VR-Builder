using System;
using System.Collections.Generic;
using VRBuilder.Core.Editor.Configuration;

namespace VRBuilder.XRInteraction.Editor.Configuration
{
    /// <summary>
    /// Configuration for the default XR interaction component.
    /// </summary>
    public class XRInteractionComponentConfiguration : IInteractionComponentConfiguration
    {
        private const string UseHandTrackingKey = "use-hand-tracking";
        private const string ControllerRigPrefab = "VRB_XR_Setup";
        private const string HandTrackingRigPrefab = "VRB_XR_Setup_Hands";

        /// <inheritdoc/>
        public string DisplayName => "XRI Integration";

        /// <inheritdoc/>
        public bool IsXRInteractionComponent => true;

        /// <inheritdoc/>
        [Obsolete("Use GetRigResourcesPath instead")]
        public string DefaultRigPrefab
        {
            get
            {
                return GetRigResourcesPath(null);
            }
        }

        public Dictionary<string, Parameter> ParametersTemplate
        {
            get
            {
                Dictionary<string, Parameter> customParams = new Dictionary<string, Parameter>();
                customParams.Add(UseHandTrackingKey, new Parameter("Use hand tracking", typeof(bool), IsHandTrackingDisabled, "Tooltip"));
                return customParams;
            }
        }

        /// <inheritdoc/>
        public string GetRigResourcesPath(Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey(UseHandTrackingKey) || parameters[UseHandTrackingKey] == null || (bool)parameters[UseHandTrackingKey] == false)
            {
                return ControllerRigPrefab;
            }
            else
            {
                return HandTrackingRigPrefab;
            }
        }

        private bool IsHandTrackingDisabled()
        {
            // TODO check if hand tracking is supported 
            return false;
        }
    }
}