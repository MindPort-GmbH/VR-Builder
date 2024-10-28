using UnityEditor;
using UnityEditor.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using VRBuilder.XRInteraction.Interactables;

namespace VRBuilder.XRInteraction.Editor.UI.Inspector
{
    [CustomEditor(typeof(TeleportationAreaVRBuilder)), CanEditMultipleObjects]
    public class TeleportationAreaVRBuilderEditor : TeleportationAreaEditor
    {
        private const string teleportLayerName = "Teleport";
        private const string reticlePrefab = "TeleportReticle";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Configure VR Builder Defaults"))
            {
                foreach (UnityEngine.Object targetObject in serializedObject.targetObjects)
                {
                    if (targetObject is TeleportationAreaVRBuilder teleportationArea)
                    {
                        ConfigureVRBuilderDefaults(teleportationArea);
                    }
                }
            }
        }

        protected virtual void ConfigureVRBuilderDefaults(TeleportationAreaVRBuilder teleportationArea)
        {
            teleportationArea.teleportTrigger = BaseTeleportationInteractable.TeleportTrigger.OnSelectExited;

            teleportationArea.ConfigureLayers(teleportLayerName, teleportLayerName);

            teleportationArea.customReticle = Resources.Load<GameObject>(reticlePrefab);

            EditorUtility.SetDirty(teleportationArea);
        }
    }
}