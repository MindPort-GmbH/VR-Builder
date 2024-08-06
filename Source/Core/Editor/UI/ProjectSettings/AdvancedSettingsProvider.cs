using UnityEditor;

namespace VRBuilder.Editor.UI
{
    public class AdvancedSettingsProvider : BaseSettingsProvider
    {
        const string Path = "Project/VR Builder/Advanced";

        public AdvancedSettingsProvider() : base(Path, SettingsScope.Project)
        {
        }

        protected override void InternalDraw(string searchContext)
        {
        }

        [SettingsProvider]
        public static SettingsProvider Provider()
        {
            SettingsProvider provider = new AdvancedSettingsProvider();
            return provider;
        }
    }
}
