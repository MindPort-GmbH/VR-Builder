using UnityEngine;

namespace VRBuilder.Core.Settings
{
    /// <summary>
    /// Settings object holding global settings for a specific scene component.
    /// </summary>
    /// <typeparam name="TObject">The component these settings are for.</typeparam>
    /// <typeparam name="TSettings">The scriptable object holding the global settings.</typeparam>
    public abstract class ComponentSettings<TObject, TSettings> : SettingsObject<TSettings> where TSettings : ScriptableObject, new() where TObject : UnityEngine.Object
    {
        private static TSettings settings;

        public abstract void ApplySettings(TObject target);

        private static TSettings RetrieveSettings()
        {
            return Instance;
        }
    }
}
