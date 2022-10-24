using UnityEditor;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.Core.UI
{
    [CustomEditor(typeof(ProcessSceneObject))]
    public class ProcessSceneObjectInspector : UnityEditor.Editor
    {
        private ProcessSceneObject sceneObject;        
       
        public override void OnInspectorGUI()
        {            
            sceneObject = target as ProcessSceneObject;

            sceneObject.na
            EditorGUILayout.HelpBox("test", MessageType.Info);
            //base.OnInspectorGUI();
        }
    }
}
