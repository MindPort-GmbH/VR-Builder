using System.IO;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Editor.Setup;
using VRBuilder.XRInteraction.Configuration;
#if VR_BUILDER_SPATIAL_KEYBOARD_SAMPLE
using VRBuilder.Core.UI.Keyboard;
using VRBuilder.XRInteraction.UI.Keyboard;
#endif

namespace VRBuilder.XRInteraction.Editor.Setup
{
    /// <summary>
    /// Scene-setup action that, when the spatial keyboard checkbox is on, instantiates the XRI Global
    /// Keyboard Manager prefab and attaches the VR Builder keyboard backend + UI Toolkit bridge onto the
    /// just-instantiated instance. The bridge auto-discovers a <see cref="UnityEngine.UIElements.UIDocument"/>
    /// in the scene at runtime; scenes that own a specific UIDocument (e.g. the Netcode connection window)
    /// call <see cref="VRBuilder.Core.UI.Keyboard.UITKKeyboardBridge.SetUIDocument"/> from their own
    /// scene-setup actions to point the bridge at it. Used identically by single-user and multi-user
    /// configurations.
    /// </summary>
    public class SpatialKeyboardSceneSetup : SceneSetup
    {
        private const string XriGlobalKeyboardManagerPrefabName = "XRI Global Keyboard Manager";

        /// <summary>
        /// Run early so the keyboard bridge GameObject exists before scene-setup actions that own a
        /// UIDocument (e.g. <c>ConnectionWindowSceneSetup</c>) try to register their TextField names
        /// against it. Setups in this codebase use 0 as the default and 10+ for rig/interaction setups,
        /// so 5 leaves room above 0 (RuntimeConfigurationSetup) and stays below the rig setups.
        /// </summary>
        public override int Priority { get; } = 5;

        /// <inheritdoc/>
        public override void Setup(ISceneSetupConfiguration configuration)
        {
#if VR_BUILDER_SPATIAL_KEYBOARD_SAMPLE
            if (!IsKeyboardEnabled(configuration))
            {
                return;
            }

            GameObject manager = null;
            try
            {
                manager = SetupPrefab(XriGlobalKeyboardManagerPrefabName, configuration.ParentObjectsHierarchy);
            }
            catch (FileNotFoundException)
            {
                Debug.LogWarning($"Scene setup could not find '{XriGlobalKeyboardManagerPrefabName}' prefab. Import the XRI Spatial Keyboard sample for the VR keyboard to work.");
                return;
            }

            if (manager == null)
            {
                // Prefab was already present in the scene; locate the existing instance so we can wire
                // the bridge onto it (idempotent — GetComponent guards prevent duplicates).
                manager = GameObject.Find(XriGlobalKeyboardManagerPrefabName);
                if (manager == null)
                {
                    return;
                }
            }

            AttachKeyboardBridge(manager);
            EditorUtility.SetDirty(manager);
#endif
        }

        /// <summary>
        /// Returns true when the scene-setup configuration has the spatial keyboard checkbox enabled.
        /// Exposed so other setup actions (e.g. scene-specific UIToolkit hosts) can gate their own
        /// keyboard wiring on the same flag.
        /// </summary>
        public static bool IsKeyboardEnabled(ISceneSetupConfiguration configuration)
        {
            if (!configuration.CustomSettings.TryGetValue(XRInteractionComponentConfiguration.UseSpatialKeyboardKey, out SceneSetupParameter parameter))
            {
                return false;
            }

            return parameter.Value is bool enabled && enabled;
        }

#if VR_BUILDER_SPATIAL_KEYBOARD_SAMPLE
        private static void AttachKeyboardBridge(GameObject host)
        {
            // Add backend before bridge so the bridge can resolve it via the IKeyboardBackend lookup.
            XriSpatialKeyboardBackend backend = host.GetComponent<XriSpatialKeyboardBackend>();
            if (backend == null)
            {
                backend = host.AddComponent<XriSpatialKeyboardBackend>();
            }

            UITKKeyboardBridge bridge = host.GetComponent<UITKKeyboardBridge>();
            if (bridge == null)
            {
                bridge = host.AddComponent<UITKKeyboardBridge>();
            }

            bridge.SetBackendBehaviour(backend);
            bridge.CloseKeyboardOnFocusOut = false;
            bridge.CloseKeyboardOnSubmit = true;
        }
#endif
    }
}
