using UnityEditor;
using UnityEngine;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.Core.UI
{
    [CustomEditor(typeof(ProcessSceneObject))]
    public class ProcessSceneObjectInspector : UnityEditor.Editor
    {
        private ProcessSceneObject sceneObject;

        public override void OnInspectorGUI()
        {
            Debug.Log("Oninspectorgui");
            sceneObject = target as ProcessSceneObject;

            string name = EditorGUILayout.TextField("Unique name", sceneObject.UniqueName);

            if(name != sceneObject.UniqueName)
            {
                sceneObject.ChangeUniqueName(name);
            }

            EditorGUILayout.HelpBox("test", MessageType.Info);
            //base.OnInspectorGUI();
        }
    }
}
