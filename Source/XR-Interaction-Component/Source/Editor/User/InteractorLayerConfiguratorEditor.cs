using UnityEditor;
using UnityEngine;
using VRBuilder.XRInteraction.User;

namespace VRBuilder.Editor.XRInteraction.User
{
    /// <summary>
    /// Editor for <see cref="InteractorLayerConfigurator"/>, adding a button 
    /// to perform layer setup manually.
    /// </summary>
    [CustomEditor(typeof(InteractorLayerConfigurator))]
    public class InteractorLayerConfiguratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            InteractorLayerConfigurator configurator = (InteractorLayerConfigurator)target;

            if (GUILayout.Button("Assign Layers"))
            {
                configurator.ExecuteSetup();
            }
        }
    }
}