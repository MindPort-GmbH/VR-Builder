using UnityEditor;
using UnityEngine;
using VRBuilder.XRInteraction.User;

namespace VRBuilder.Editor.XRInteraction.User
{
    [CustomEditor(typeof(InteractorLayerConfigurator))]

    public class InteractorLayerConfiguratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            InteractorLayerConfigurator configurator = (InteractorLayerConfigurator)target;

            if (GUILayout.Button("Setup"))
            {
                configurator.ExecuteSetup();
            }
        }
    }
}