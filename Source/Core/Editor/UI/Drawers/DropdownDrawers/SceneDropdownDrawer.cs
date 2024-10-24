using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Drawer for a dropdown listing all the scenes in the build settings and allowing to select one by index.
    /// </summary>
    public class SceneDropdownDrawer : DropdownDrawer<string>
    {
        /// <inheritdoc/>
        protected override IList<DropDownElement<string>> PossibleOptions => options;

        private List<DropDownElement<string>> options = new List<DropDownElement<string>>();

        public SceneDropdownDrawer()
        {
            BuildSceneList();
            EditorBuildSettings.sceneListChanged += BuildSceneList;
        }

        private void BuildSceneList()
        {
            options.Clear();
            options.Add(new DropDownElement<string>(null, "<No scene selected>"));
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                options.Add(new DropDownElement<string>(scene.path, Path.GetFileNameWithoutExtension(scene.path)));
            }
        }
    }
}
