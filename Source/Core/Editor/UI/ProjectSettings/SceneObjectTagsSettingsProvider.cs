using UnityEditor;
using VRBuilder.Core.Internationalization;
using VRBuilder.Core.Settings;

namespace VRBuilder.Editor.UI
{
    public class SceneObjectTagsSettingsProvider : BaseSettingsProvider
    {
        const string Path = "Project/VR Builder/Scene Object Tags";

        public SceneObjectTagsSettingsProvider() : base(Path, SettingsScope.Project)
        {
        }

        protected override void InternalDraw(string searchContext)
        {
            SceneObjectTags config = SceneObjectTags.Instance;
            UnityEditor.Editor.CreateEditor(config).OnInspectorGUI();
        }

        public override void OnDeactivate()
        {
            if (EditorUtility.IsDirty(SceneObjectTags.Instance))
            {
                LanguageSettings.Instance.Save();
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
