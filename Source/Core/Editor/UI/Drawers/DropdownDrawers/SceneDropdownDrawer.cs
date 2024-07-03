using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace VRBuilder.Editor.UI.Drawers
{
    public class SceneDropdownDrawer : DropdownDrawer<int>
    {
        protected override IList<DropDownElement<int>> PossibleOptions => options;

        private List<DropDownElement<int>> options = new List<DropDownElement<int>>();

        public SceneDropdownDrawer()
        {
            BuildSceneList();
            EditorBuildSettings.sceneListChanged += BuildSceneList;
        }

        private void BuildSceneList()
        {
            options.Clear();
            options.Add(new DropDownElement<int>(-1, "<No scene selected>"));
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                options.Add(new DropDownElement<int>(SceneUtility.GetBuildIndexByScenePath(scene.path), Path.GetFileNameWithoutExtension(scene.path)));
            }
        }
    }
}
