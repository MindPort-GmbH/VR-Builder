using UnityEditor;
using UnityEditor.XR.Interaction.Toolkit;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VRBuilder.XRInteraction;

namespace VRBuilder.Editor.XRInteraction
{
    [CustomEditor(typeof(TeleportationAnchorVRBuilder)), CanEditMultipleObjects]
    public class TeleportationAnchorVRBuilderEditor : TeleportationAnchorEditor
    {
        private const string teleportLayerName = "XR Teleport";
        private const string reticlePrefab = "TeleportReticle";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Configure VR Builder defaults"))
            {
                foreach (UnityEngine.Object targetObject in serializedObject.targetObjects)
                {
                    if (targetObject is TeleportationAnchorVRBuilder teleportationAnchor)
                    {
                        ConfigureVRBuilderDefaults(teleportationAnchor);
                    }
                }
            }
        }

        protected virtual void ConfigureVRBuilderDefaults(TeleportationAnchorVRBuilder teleportationAnchor)
        {
            teleportationAnchor.teleportTrigger = BaseTeleportationInteractable.TeleportTrigger.OnDeactivated;

            InteractionLayerMask teleportLayer = InteractionLayerMask.NameToLayer(teleportLayerName);
            LayerMask teleportRaycastLayer = LayerMask.NameToLayer(teleportLayerName);

            teleportationAnchor.gameObject.layer = teleportRaycastLayer;
            teleportationAnchor.interactionLayers = 1 << teleportLayer;

            teleportationAnchor.customReticle = Resources.Load<GameObject>(reticlePrefab);

            EditorUtility.SetDirty(teleportationAnchor);
        }
    }
}