using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Input
{
    public class InputSettings : SettingsObject<InputSettings>, IInputConfiguration
    {
        public string DefaultInputActionAssetPath { get; } = "KeyBindings/BuilderDefaultKeyBindings";
        public string CustomInputActionAssetPath { get; } = "KeyBindings/BuilderCustomKeyBindings";
    }
}