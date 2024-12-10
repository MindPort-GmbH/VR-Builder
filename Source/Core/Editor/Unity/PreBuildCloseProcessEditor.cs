using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using VRBuilder.Core.Editor.UI.Windows;

namespace VRBuilder.Core.Editor.Unity
{
    /// <summary>
    /// We are closing the process editor window before the build starts.
    /// Having an open process editor window can lead to corrupt Unique Names.
    /// </summary>
    public class PreBuildCloseProcessEditor : IPreprocessBuildWithReport
    {
        ///<inheritdoc />
        public int callbackOrder => 2;

        ///<inheritdoc />
        public void OnPreprocessBuild(BuildReport report)
        {
            if (WindowUtils.CloseProcessEditorWindow())
            {
                UnityEngine.Debug.Log("Process Editor Window was closed before building as it can lead to corrupt Unique Names.");
            }
        }
    }
}
