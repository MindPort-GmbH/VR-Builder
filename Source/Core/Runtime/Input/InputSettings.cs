using UnityEngine;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Input
{
    public class InputSettings : SettingsObject<InputSettings>, IInputConfiguration
    {
        [SerializeField]
        private string defaultInputActionAssetPath = "KeyBindings/BuilderDefaultKeyBindings";

        [SerializeField]
        private string customInputActionAssetPath = "KeyBindings/BuilderCustomKeyBindings";

        public string DefaultInputActionAssetPath => defaultInputActionAssetPath;

        public string CustomInputActionAssetPath => customInputActionAssetPath;
    }
}