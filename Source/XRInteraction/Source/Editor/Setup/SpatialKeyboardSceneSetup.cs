using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.Setup;
using VRBuilder.XRInteraction.Configuration;
#if VR_BUILDER_SPATIAL_KEYBOARD_SAMPLE
using VRBuilder.Core.UI.Keyboard;
using VRBuilder.XRInteraction.UI.Keyboard;
#endif

namespace VRBuilder.XRInteraction.Editor.Setup
{
    /// <summary>
    /// Spawns the XRI Global Keyboard Manager prefab when the scene-setup checkbox is on, and
    /// creates a default UIDocument host that already has the keyboard backend and bridge components
    /// attached. Single-user scenes use this class directly: the user just needs to assign a UXML to
    /// the spawned UIDocument and any TextField will work with the spatial keyboard. Multi-user scenes
    /// inherit and override <see cref="WireSceneSpecificTargets"/> to wire the existing connection UI
    /// instead of creating a generic host.
    /// </summary>
    public class SpatialKeyboardSceneSetup : SceneSetup
    {
        private const string XriGlobalKeyboardManagerPrefabName = "XRI Global Keyboard Manager";
        private const string DefaultUIDocumentHostName = "VR Builder UI";

        /// <inheritdoc/>
        public override void Setup(ISceneSetupConfiguration configuration)
        {
#if VR_BUILDER_SPATIAL_KEYBOARD_SAMPLE
            if (!IsKeyboardEnabled(configuration))
            {
                return;
            }

            EnsureGlobalKeyboardManagerPrefab();
            WireSceneSpecificTargets(configuration);
#endif
        }

        /// <summary>
        /// Default behaviour creates a fresh GameObject with an empty <see cref="UIDocument"/> plus the
        /// keyboard backend and bridge components, ready for the user to drop in their own UXML.
        /// Derived classes (e.g. multi-user) override this to wire an existing UI host instead.
        /// </summary>
        protected virtual void WireSceneSpecificTargets(ISceneSetupConfiguration configuration)
        {
#if VR_BUILDER_SPATIAL_KEYBOARD_SAMPLE
            if (GameObject.Find(DefaultUIDocumentHostName) != null)
            {
                return;
            }

            GameObject host = new GameObject(DefaultUIDocumentHostName);
            host.AddComponent<UIDocument>();
            XriSpatialKeyboardBackend backend = host.AddComponent<XriSpatialKeyboardBackend>();
            UITKKeyboardBridge bridge = host.AddComponent<UITKKeyboardBridge>();
            bridge.SetBackendBehaviour(backend);
            bridge.CloseKeyboardOnFocusOut = false;
            bridge.CloseKeyboardOnSubmit = true;

            EditorUtility.SetDirty(host);
#endif
        }

        protected static bool IsKeyboardEnabled(ISceneSetupConfiguration configuration)
        {
            if (!configuration.CustomSettings.TryGetValue(XRInteractionComponentConfiguration.UseSpatialKeyboardKey, out SceneSetupParameter parameter))
            {
                return false;
            }

            return parameter.Value is bool enabled && enabled;
        }

#if VR_BUILDER_SPATIAL_KEYBOARD_SAMPLE
        private static void EnsureGlobalKeyboardManagerPrefab()
        {
            if (GameObject.Find(XriGlobalKeyboardManagerPrefabName) != null)
            {
                return;
            }

            string[] guids = AssetDatabase.FindAssets($"t:Prefab {XriGlobalKeyboardManagerPrefabName}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset != null && asset.name == XriGlobalKeyboardManagerPrefabName)
                {
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
                    if (instance != null)
                    {
                        instance.name = XriGlobalKeyboardManagerPrefabName;
                    }
                    return;
                }
            }

            Debug.LogWarning($"Scene setup could not find '{XriGlobalKeyboardManagerPrefabName}' prefab. Import the XRI Spatial Keyboard sample for the VR keyboard to work.");
        }
#endif
    }
}
