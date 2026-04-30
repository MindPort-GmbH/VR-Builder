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
    /// Keyboard Manager prefab and creates a default GameObject carrying the keyboard backend and bridge
    /// components. The bridge auto-resolves a <see cref="UIDocument"/> anywhere in the scene at runtime,
    /// or can be pointed at a specific one via its inspector field — single-user scenes don't have to
    /// pre-decide where the UIToolkit document lives. Used identically by single-user and multi-user
    /// configurations; scenes that own additional UIToolkit hosts (e.g. the Netcode connection window)
    /// wire those hosts up from their own scene-setup actions.
    /// </summary>
    public class SpatialKeyboardSceneSetup : SceneSetup
    {
        private const string XriGlobalKeyboardManagerPrefabName = "XRI Global Keyboard Manager";
        private const string DefaultUIDocumentHostName = "UIToolkit Spatial Keyboard Bridge";

        /// <inheritdoc/>
        public override void Setup(ISceneSetupConfiguration configuration)
        {
#if VR_BUILDER_SPATIAL_KEYBOARD_SAMPLE
            if (!IsKeyboardEnabled(configuration))
            {
                return;
            }

            try
            {
                SetupPrefab(XriGlobalKeyboardManagerPrefabName, configuration.ParentObjectsHierarchy);
            }
            catch (FileNotFoundException)
            {
                Debug.LogWarning($"Scene setup could not find '{XriGlobalKeyboardManagerPrefabName}' prefab. Import the XRI Spatial Keyboard sample for the VR keyboard to work.");
            }

            CreateKeyboardBridgeHost(configuration);
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
        private void CreateKeyboardBridgeHost(ISceneSetupConfiguration configuration)
        {
            if (GameObject.Find(DefaultUIDocumentHostName) != null)
            {
                return;
            }

            GameObject host = new GameObject(DefaultUIDocumentHostName);
            XriSpatialKeyboardBackend backend = host.AddComponent<XriSpatialKeyboardBackend>();
            UITKKeyboardBridge bridge = host.AddComponent<UITKKeyboardBridge>();
            bridge.SetBackendBehaviour(backend);
            bridge.CloseKeyboardOnFocusOut = false;
            bridge.CloseKeyboardOnSubmit = true;

            SetPrefabParent(host, configuration.ParentObjectsHierarchy);
            EditorUtility.SetDirty(host);
        }
#endif
    }
}
