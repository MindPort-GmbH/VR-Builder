using UnityEditor;

namespace VRBuilder.Editor.UI
{
    public class ComponentSettingsProvider : BaseSettingsProvider
    {
        const string Path = "Project/VR Builder/Component Settings";

        private UnityEditor.Editor editor;

        public ComponentSettingsProvider() : base(Path, SettingsScope.Project) { }

        [SettingsProvider]
        public static SettingsProvider Provider()
        {
            SettingsProvider provider = new ComponentSettingsProvider();
            return provider;
        }

        protected override void InternalDraw(string searchContext)
        {
        }
    }
}
