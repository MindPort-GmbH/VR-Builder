using UnityEditor;

using UnityEngine;

using VRBuilder.XRInteraction;

namespace VRBuilder.Editor.XRInteraction
{
    [CustomEditor(typeof(TeleportationAreaVRBuilder)), CanEditMultipleObjects]
    public class TeleportationAreaVRBuilderEditor : UnityEditor.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAreaEditor
    {
        private const string teleportLayerName = "XR Teleport";
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
            teleportationArea.teleportTrigger = UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.BaseTeleportationInteractable.TeleportTrigger.OnDeactivated;

            teleportationArea.ConfigureLayers(teleportLayerName, teleportLayerName);

            teleportationArea.customReticle = Resources.Load<GameObject>(reticlePrefab);

            EditorUtility.SetDirty(teleportationArea);
        }
    }
}