using System;

namespace VRBuilder.Editor.UI
{
    public class SceneObjectTagsSettingsSection : IProjectSettingsSection
    {
        public string Title => "Scene Object Tags";

        public Type TargetPageProvider => typeof(SceneObjectTagsSettingsProvider);

        public int Priority => 64;

        public void OnGUI(string searchContext)
        {
        }
    }
}
