using System;

namespace VRBuilder.Core.Editor.Configuration
{
    /// <summary>
    /// A custom settable parameter for a configuration.
    /// </summary>
    public struct ConfigurationSetting
    {
        /// <summary>
        /// The display label of the setting, shown in the UI.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// A tooltip describing the setting, shown in the UI.
        /// </summary>
        public string Tooltip { get; private set; }

        /// <summary>
        /// Function that returns true if the setting should be disabled in the UI.
        /// If null, the setting is always enabled.
        /// </summary>
        public Func<bool> IsDisabled { get; private set; }

        /// <summary>
        /// Callback function that is called when the setting value is changed.
        /// Should return the new value of the setting.
        /// Can be null if no callback is needed.
        /// </summary>
        public Action<object> ChangedCallback { get; private set; }

        public ConfigurationSetting(string label, Type type, string tooltip = "", Func<bool> isDisabled = null, Action<object> changedCallback = null)
        {
            Label = label;
            Type = type;
            Tooltip = tooltip;
            IsDisabled = isDisabled ?? (() => false);
            ChangedCallback = changedCallback;
        }
    }
}
