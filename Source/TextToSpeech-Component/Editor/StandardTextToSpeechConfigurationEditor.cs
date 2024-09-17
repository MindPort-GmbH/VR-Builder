using UnityEditor;
using UnityEngine;
using VRBuilder.TextToSpeech;

namespace Source.TextToSpeech_Component.Editor
{
    [CustomEditor(typeof(StandardTextToSpeechConfiguration))]
    public class StandardTextToSpeechConfigurationEditor: UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            //target = (StandardTextToSpeechConfiguration)target;
            //GUILayout.Button(new GUIContent(""));
            
             }
    }
}