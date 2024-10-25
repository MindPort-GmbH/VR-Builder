using UnityEditor;

namespace VRBuilder.Core.Editor.UI.ProjectSettings
{
    public class SpectatorSettingsProvider : BaseSettingsProvider
    {
        const string Path = "Project/VR Builder/Spectator";

        public SpectatorSettingsProvider() : base(Path, SettingsScope.Project)
        {
        }

        protected override void InternalDraw(string searchContext)
        {
        }

        [SettingsProvider]
        public static SettingsProvider Provider()
        {
            SettingsProvider provider = new SpectatorSettingsProvider();
            return provider;
        }
    }
}
