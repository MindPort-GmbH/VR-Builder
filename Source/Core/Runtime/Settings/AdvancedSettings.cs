using UnityEngine;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Settings
{
    /// <summary>
    /// Settings for advanced VR Builder users.
    /// </summary>
    [CreateAssetMenu(fileName = "AdvancedSettings", menuName = "VR Builder/AdvancedSettings", order = 1)]
    public class AdvancedSettings : SettingsObject<AdvancedSettings>
    {
        [SerializeField]
        [Tooltip("If true, a dialog will be shown before adding the Process Scene Object component to an object dragged in a scene object reference field.")]
        public bool AutoAddProcessSceneObject = false;

        [SerializeField]
        [Tooltip("If true, the required scene object properties will be automatically added when the object is dragged in a scene object reference field, without need for user intervention.")]
        public bool AutoAddProperties = false;

        [SerializeField]
        [Tooltip("If true, more info will be shown on the inspector of VR Builder components.")]
        public bool ShowExpertInfo = false;
    }
}
