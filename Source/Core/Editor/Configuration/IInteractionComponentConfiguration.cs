using System;
using System.Collections.Generic;

namespace VRBuilder.Core.Editor.Configuration
{
    /// <summary>
    /// Configuration for an interaction component.
    /// </summary>
    public interface IInteractionComponentConfiguration
    {
        /// <summary>
        /// Display name of the interaction component.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// True if the interaction component is meant to work with XR.
        /// </summary>
        bool IsXRInteractionComponent { get; }

        /// <summary>
        /// Name of the prefab to be spawned as user rig.
        /// </summary>
        string DefaultRigPrefab { get; }

        /// <summary>
        /// Returns the Resources path of the required rig.
        /// </summary>
        /// <param name="parameters">Custom parameters for the current configuration.</param>
        /// <returns>The Resources path of the rig.</returns>
        string GetRigResourcesPath(Dictionary<string, object> parameters);

        /// <summary>
        /// Defines the custom parameters implemented by this configuration.
        /// </summary>
        Dictionary<string, Parameter> ParametersTemplate { get; }
    }

    public struct Parameter
    {
        public string Label { get; private set; }
        public Type Type { get; private set; }
        public string Tooltip { get; private set; }
        public Func<bool> IsDisabled { get; private set; }
        public Action<object> ChangedCallback { get; private set; }

        public Parameter(string label, Type type, Func<bool> isEnabled, Action<object> changedCallback = null, string tooltip = "")
        {
            Label = label;
            Type = type;
            Tooltip = tooltip;
            IsDisabled = isEnabled ?? (() => true);
            ChangedCallback = changedCallback;
        }
    }
}
