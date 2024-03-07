using UnityEditor;
using VRBuilder.Core.Settings;

namespace VRBuilder.Editor.UI
{
    /// <summary>
    /// Provider for a list of scene object tags.
    /// </summary>
    public class SceneObjectTagsSettingsProvider : BaseSettingsProvider
    {
        const string Path = "Project/VR Builder/Scene Object Tags";

        public SceneObjectTagsSettingsProvider() : base(Path, SettingsScope.Project)
        {
        }

        protected override void InternalDraw(string searchContext)
        {
        }

        public override void OnDeactivate()
        {
            if (EditorUtility.IsDirty(SceneObjectTags.Instance))
            {
                SceneObjectTags.Instance.Save();
            }
        }

        [SettingsProvider]
        public static SettingsProvider Provider()
        {
            SettingsProvider provider = new SceneObjectTagsSettingsProvider();
            return provider;
        }
    }
}
