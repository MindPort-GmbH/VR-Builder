using UnityEngine;
using VRBuilder.Core.Settings;
using VRBuilder.Core.User;

namespace Source.Core.Runtime.Configuration
{
    [CreateAssetMenu(fileName = "UserSettings", menuName = "VR Builder/User Configuration", order = 2)]
    public class UserSettings : SettingsObject<UserSettings>, IUserConfiguration
    {
    }
}