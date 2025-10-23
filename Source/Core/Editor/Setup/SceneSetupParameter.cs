using System;

namespace VRBuilder.Core.Editor.Setup
{
    /// <summary>
    /// A custom settable parameter for a configuration.
    /// </summary>
    public class SceneSetupParameter
    {
        /// <summary>
        /// The display label of the parameter, shown in the UI.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// The current value of the parameter.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// A tooltip describing the parameter, shown in the UI.
        /// </summary>
        public string Tooltip { get; private set; }

        /// <summary>
        /// Function that returns true if the parameter should be disabled in the UI.
        /// If null, the parameter is always enabled.
        /// </summary>
        public Func<bool> IsDisabled { get; private set; }

        /// <summary>
        /// Callback function that is called when the parameter value is changed.
        /// Should return the new value of the parameter.
        /// Can be null if no callback is needed.
        /// </summary>
        public Action<object> ChangedCallback { get; private set; }

        public SceneSetupParameter(string label, Type type, object defaultValue, string tooltip = "", Func<bool> isDisabled = null, Action<object> changedCallback = null)
        {
            Label = label;
            Type = type;
            Value = defaultValue;
            Tooltip = tooltip;
            IsDisabled = isDisabled ?? (() => false);
            ChangedCallback = changedCallback;
        }
    }
}
