using UnityEditor;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Editor.UI.ProjectSettings
{
    /// <summary>
    /// Provider for a list of scene object groups.
    /// </summary>
    public class SceneObjectGroupsSettingsProvider : BaseSettingsProvider
    {
        const string Path = "Project/VR Builder/Scene Object Groups";

        public SceneObjectGroupsSettingsProvider() : base(Path, SettingsScope.Project)
        {
        }

        protected override void InternalDraw(string searchContext)
        {
        }

        public override void OnDeactivate()
        {
            if (EditorUtility.IsDirty(SceneObjectGroups.Instance))
            {
                SceneObjectGroups.Instance.Save();
            }
        }

        [SettingsProvider]
        public static SettingsProvider Provider()
        {
            SettingsProvider provider = new SceneObjectGroupsSettingsProvider();
            return provider;
        }
    }
}
