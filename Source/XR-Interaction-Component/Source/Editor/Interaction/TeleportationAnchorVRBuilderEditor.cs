using System;
using UnityEditor;
using UnityEditor.XR.Interaction.Toolkit;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;
using VRBuilder.XRInteraction;

namespace VRBuilder.Editor.XRInteraction
{
    [CustomEditor(typeof(TeleportationAnchorVRBuilder)), CanEditMultipleObjects]
    public class TeleportationAnchorVRBuilderEditor : TeleportationAnchorEditor
    {
        private const string teleportLayerName = "XR Teleport";
        private const string reticlePrefab = "TeleportReticle";
        private const string anchorPrefabName = "VRBuilderAnchorPrefab";
        private const string anchorSceneName = "Anchor";
        private const string srpMaterialPath = "Materials/AnchorMaterialSRP";
        private const string urpMaterialPath = "Materials/AnchorMaterialURP";
        private const string anchorPlaneObjectName = "Plane";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Configure VR Builder Defaults"))
            {
                foreach (UnityEngine.Object targetObject in serializedObject.targetObjects)
                {
                    if (targetObject is TeleportationAnchorVRBuilder teleportationAnchor)
                    {
                        ConfigureVRBuilderDefaults(teleportationAnchor);
                    }
                }
            }

            if (GUILayout.Button("Set Default Teleportation Anchor"))
            {
                foreach (UnityEngine.Object targetObject in serializedObject.targetObjects)
                {
                    if (targetObject is TeleportationAnchorVRBuilder teleportationAnchor)
                    {
                        ConfigureDefaultTeleportationAnchor(teleportationAnchor);
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

        private void ConfigureDefaultTeleportationAnchor(TeleportationAnchorVRBuilder teleportationAnchor)
        {
            try
            {
                if (teleportationAnchor.transform.childCount == 0)
                {
                    CreateVisualEffect(teleportationAnchor);
                }

                ConfigureVRBuilderDefaults(teleportationAnchor);
                ConfigureCollider(teleportationAnchor);
            }
            catch (Exception e)
            {
                Debug.LogError($"There was an exception of type '{e.GetType()}' when trying to setup {name} as default Teleportation Anchor\n{e.Message}", teleportationAnchor.gameObject);
            }
        }

        private GameObject CreateVisualEffect(TeleportationAnchorVRBuilder teleportationAnchor)
        {
            Transform anchorTransform = teleportationAnchor.transform;

            GameObject anchorPrefab = Instantiate(Resources.Load<GameObject>(anchorPrefabName));
            anchorPrefab.name = anchorSceneName;

            for (int i = 0; i < anchorPrefab.transform.childCount; i++)
            {
                MeshRenderer meshRenderer = anchorPrefab.transform.GetChild(i).GetComponent<MeshRenderer>();

                if (meshRenderer != null && meshRenderer.gameObject.name == anchorPlaneObjectName)
                {
                    meshRenderer.sharedMaterial = GraphicsSettings.currentRenderPipeline ? Resources.Load<Material>(urpMaterialPath) : Resources.Load<Material>(srpMaterialPath);
                }
            }

            anchorPrefab.transform.SetPositionAndRotation((anchorTransform.position + (Vector3.up * 0.01f)), anchorTransform.rotation);
            anchorPrefab.transform.SetParent(anchorTransform);

            return anchorPrefab;
        }

        private void ConfigureCollider(TeleportationAnchorVRBuilder teleportationAnchor)
        {
            BoxCollider anchorCollider = teleportationAnchor.GetComponent<BoxCollider>();

            if (anchorCollider == null)
            {
                anchorCollider = teleportationAnchor.gameObject.AddComponent<BoxCollider>();
            }

            anchorCollider.center = new Vector3(0f, 0.02f, 0f);
            anchorCollider.size = new Vector3(1f, 0.01f, 1f);
        }
    }
}