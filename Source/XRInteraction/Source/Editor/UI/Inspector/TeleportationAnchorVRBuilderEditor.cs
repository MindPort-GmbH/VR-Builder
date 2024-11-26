using System;
using UnityEditor;
using UnityEditor.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using VRBuilder.Core.Utils;
using VRBuilder.XRInteraction.Interactables;

namespace VRBuilder.XRInteraction.Editor.UI.Inspector
{
    [CustomEditor(typeof(TeleportationAnchorVRBuilder)), CanEditMultipleObjects]
    public class TeleportationAnchorVRBuilderEditor : TeleportationAnchorEditor
    {
        private const string teleportLayerName = "Teleport";
        private const string reticlePrefab = "TeleportReticle";
        private const string anchorPrefabName = "VRBuilderAnchorPrefab";
        private const string anchorSceneName = "Anchor";
        private const string srpMaterialPath = "Materials/AnchorMaterialSRP";
        private const string urpMaterialPath = "Materials/AnchorMaterialURP";
        private const string anchorPlaneObjectName = "Plane";
        private const string snapVolumePrefabName = "Interactables/VRBuilderTeleportationSnapVolumePrefab";
        private const string snapVolumeSceneName = "Snap Volume";
        private const string interactionAffordancePrefabName = "Interactables/VRBuilderTeleportInteractionAffordancePrefab";
        private const string interactionAffordanceSceneName = "Interaction Affordance";
        private const string proximityEntryPrefabName = "Interactables/VRBuilderTeleportationAnchorProximityEntryPrefab";
        private const string proximityEntrySceneName = "Proximity Entry";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            HandleButtonClick("Configure VR Builder Defaults", ConfigureVRBuilderDefaults);
            HandleButtonClick("Set Default Teleportation Anchor", ConfigureDefaultTeleportationAnchor);
            HandleButtonClick($"Add {snapVolumeSceneName}", CreateSnapVolume);
            HandleButtonClick($"Add {interactionAffordanceSceneName}", CreateInteractionAffordance);
            HandleButtonClick($"Add {proximityEntrySceneName}", ConfigureTeleportationProximityEntry);
        }

        private void HandleButtonClick(string buttonLabel, Action<TeleportationAnchorVRBuilder> action)
        {
            if (GUILayout.Button(buttonLabel))
            {
                foreach (UnityEngine.Object targetObject in serializedObject.targetObjects)
                {
                    if (targetObject is TeleportationAnchorVRBuilder teleportationAnchor)
                    {
                        action(teleportationAnchor);
                    }
                }
            }
        }

        protected virtual void ConfigureVRBuilderDefaults(TeleportationAnchorVRBuilder teleportationAnchor)
        {
            teleportationAnchor.teleportTrigger = BaseTeleportationInteractable.TeleportTrigger.OnSelectExited;

            teleportationAnchor.ConfigureLayers(teleportLayerName, teleportLayerName);

            teleportationAnchor.customReticle = Resources.Load<GameObject>(reticlePrefab);

            EditorUtility.SetDirty(teleportationAnchor);
        }

        protected void ConfigureDefaultTeleportationAnchor(TeleportationAnchorVRBuilder teleportationAnchor)
        {
            try
            {
                ConfigureVRBuilderDefaults(teleportationAnchor);
                ConfigureCollider(teleportationAnchor);

                teleportationAnchor.teleportAnchorTransform = CreateVisualEffect(teleportationAnchor).transform;
            }
            catch (Exception e)
            {
                Debug.LogError($"There was an exception of type '{e.GetType()}' when trying to setup {name} as default Teleportation Anchor\n{e.Message}", teleportationAnchor.gameObject);
            }
        }

        protected virtual void CreateSnapVolume(TeleportationAnchorVRBuilder teleportationAnchor)
        {
            CreateChildObject(teleportationAnchor, snapVolumePrefabName, snapVolumeSceneName, (affordancePrefab) =>
            {
                affordancePrefab.GetComponent<XRInteractableSnapVolume>().interactableObject = teleportationAnchor;
            });
        }

        protected virtual void CreateInteractionAffordance(TeleportationAnchorVRBuilder teleportationAnchor)
        {
            CreateChildObject(teleportationAnchor, interactionAffordancePrefabName, interactionAffordanceSceneName, (affordancePrefab) =>
            {
                //TODO fix that state provider if unity adds the new implementation
                #pragma warning disable CS0618 // Type or member is obsolete
                affordancePrefab.GetComponent<XRInteractableAffordanceStateProvider>().interactableSource = teleportationAnchor;
                #pragma warning restore CS0618 // Type or member is obsolete
            });
        }

        protected virtual void ConfigureTeleportationProximityEntry(TeleportationAnchorVRBuilder teleportationAnchor)
        {
            CreateChildObject(teleportationAnchor, proximityEntryPrefabName, proximityEntrySceneName, null);
        }

        private void CreateChildObject(TeleportationAnchorVRBuilder teleportationAnchor, string prefabName, string sceneName, Action<GameObject> additionalSetup)
        {
            try
            {
                teleportationAnchor.gameObject.RemoveChildWithNameImmediate(sceneName);

                GameObject affordancePrefab = Instantiate(Resources.Load<GameObject>(prefabName));
                affordancePrefab.name = sceneName;

                Transform anchorTransform = teleportationAnchor.transform;
                affordancePrefab.SetLayer<Transform>(anchorTransform.gameObject.layer, true);
                affordancePrefab.transform.SetPositionAndRotation(anchorTransform.position, anchorTransform.rotation);
                affordancePrefab.transform.SetParent(anchorTransform);

                additionalSetup?.Invoke(affordancePrefab);
            }
            catch (Exception e)
            {
                Debug.LogError($"There was an exception of type '{e.GetType()}' when trying to setup {name} with {sceneName} \n{e.Message}", teleportationAnchor.gameObject);
            }
        }

        private GameObject CreateVisualEffect(TeleportationAnchorVRBuilder teleportationAnchor)
        {
            teleportationAnchor.gameObject.RemoveChildWithNameImmediate(anchorSceneName);

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

            Transform anchorTransform = teleportationAnchor.transform;
            anchorPrefab.SetLayer<Transform>(anchorTransform.gameObject.layer, true);
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