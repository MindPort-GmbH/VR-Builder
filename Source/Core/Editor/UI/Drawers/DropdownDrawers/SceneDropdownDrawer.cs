using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace VRBuilder.Editor.UI.Drawers
{
    public class SceneDropdownDrawer : DropdownDrawer<string>
    {
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
                string name = Path.GetFileNameWithoutExtension(scene.path);
                options.Add(new DropDownElement<string>(scene.path, name));
            }
        }
    }
}
