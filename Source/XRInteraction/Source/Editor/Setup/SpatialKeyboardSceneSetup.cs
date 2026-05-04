using System.IO;
using UnityEngine;
using VRBuilder.Core.Editor.Setup;
using VRBuilder.XRInteraction.Configuration;

namespace VRBuilder.XRInteraction.Editor.Setup
{
    /// <summary>
    /// Scene-setup action that, when the spatial keyboard checkbox is on, instantiates the XRI Global
    /// Keyboard Manager prefab. Scene-setup actions that own a UIDocument and need the in-VR keyboard
    /// (e.g. <c>ConnectionWindowSceneSetup</c>) are responsible for attaching their own
    /// <c>UITKKeyboardBridge</c> + backend onto the GameObject they manage.
    /// </summary>
    public class SpatialKeyboardSceneSetup : SceneSetup
    {
        private const string XriGlobalKeyboardManagerPrefabName = "XRI Global Keyboard Manager";

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
    }
}
