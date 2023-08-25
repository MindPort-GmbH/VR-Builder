using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using VRBuilder.Editor.UI.Windows;

namespace VRBuilder.Editor
{
    /// <summary>
    /// We are closing the process editor window before the build starts.
    /// Having an open process editor window can lead to corrupt Unique Names.
    /// </summary>
    public class PreBuildCloseProcessEditor : IPreprocessBuildWithReport
    {
        ///<inheritdoc />
        public int callbackOrder => 1;

        ///<inheritdoc />
        public void OnPreprocessBuild(BuildReport report)
        {
            if (WindowUtils.CloseProcessEditorWindow())
            {
                Debug.Log("Process Editor Window was closed before building as it can lead to corrupt Unique Names.");
            }
        }
    }
}
