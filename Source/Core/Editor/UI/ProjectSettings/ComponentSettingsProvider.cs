using UnityEditor;

namespace VRBuilder.Core.Editor.UI.ProjectSettings
{
    /// <summary>
    /// Settings provider that groups global settings for scene components in separate sections.
    /// </summary>
    public class ComponentSettingsProvider : BaseSettingsProvider
    {
        private const string Path = "Project/VR Builder/Component Settings";

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
