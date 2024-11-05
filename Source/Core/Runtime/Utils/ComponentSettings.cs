using System;
using UnityEngine;

namespace VRBuilder.Core.Runtime.Utils
{
    /// <summary>
    /// Settings object holding global settings for a specific scene component.
    /// </summary>
    /// <typeparam name="TObject">The component these settings are for.</typeparam>
    /// <typeparam name="TSettings">The scriptable object holding the global settings.</typeparam>
    public abstract class ComponentSettings<TObject, TSettings> : SettingsObject<TSettings> where TSettings : ScriptableObject, new() where TObject : UnityEngine.Object
    {
        private static TSettings settings;

        /// <summary>
        /// Loads the first existing settings found in the project.
        /// If non are found, it creates and saves a new instance with default values.
        /// </summary>
        [Obsolete("This is redundant and will be removed in a future major version. Use SettingsObject.Instance instead. Note that saving settings outside a Resources folder is no longer supported.")]
        public static TSettings Settings => RetrieveSettings();

        public abstract void ApplySettings(TObject target);

        private static TSettings RetrieveSettings()
        {
            return Instance;
        }
    }
}
