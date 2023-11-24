using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Setup;
using VRBuilder.Editor.XRInteractionExtension;

namespace VRBuilder.Editor.XRInteraction.Menu
{
    /// <summary>
    /// Menu entries for updating layer configuration on game objects.
    /// </summary>
    internal static class ConfigureInteractionLayersMenuEntry
    {
        private const string teleportRaycastLayer = "XR Teleport";
        private const string teleportInteractionLayer = "XR Teleport";

        [MenuItem("Tools/VR Builder/Developer/Configure Teleportation Layers", false, 80)]
        private static void ConfigureTeleportationLayers()
        {
            if (InteractionLayerUtils.AddLayerIfNotPresent(teleportInteractionLayer) == false)
            {
                Debug.LogError($"Interaction layer '{teleportInteractionLayer}' was not found and it was not possible to add it automatically.");
            }

            IEnumerable<ILayerConfigurator> configurators = GameObject.FindObjectsOfType<GameObject>(true).
                Where(go => go.GetComponent<ILayerConfigurator>() != null).
                Select(go => go.GetComponent<ILayerConfigurator>()).
                Where(configurator => configurator.LayerSet == LayerSet.Teleportation);

            if (configurators.Count() == 0)
            {
                Debug.Log("No objects found to update.");
            }

            if (EditorUtility.DisplayDialog("Configure Teleportation Layers",
                "This will update all supported objects to use the default teleportation layers.\n" +
                "This will overwrite raycast and interaction layer masks on supported interactors and interactables.\n" +
                "Proceed?", "Yes", "No"))
            {
                foreach (ILayerConfigurator configurator in configurators)
                {
                    configurator.ConfigureLayers(teleportInteractionLayer, teleportRaycastLayer);
                }
            }
        }
    }
}
