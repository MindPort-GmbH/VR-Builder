using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Setup;

namespace VRBuilder.XRInteraction.Editor.UI.Menu
{
    /// <summary>
    /// Menu entries for updating layer configuration on game objects.
    /// </summary>
    internal static class ConfigureInteractionLayersMenuEntry
    {
        private const string teleportRaycastLayer = "Teleport";
        private const string teleportInteractionLayer = "Teleport";

        [MenuItem("Tools/VR Builder/Developer/Configure Teleportation Layers", false, 80)]
        private static void ConfigureTeleportationLayers()
        {
            IEnumerable<GameObject> configuratorGameObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).
                Where(go => go.GetComponent<ILayerConfigurator>() != null);

            if (configuratorGameObjects.Count() == 0)
            {
                Debug.Log("No objects found to update.");
            }

            if (EditorUtility.DisplayDialog("Configure Teleportation Layers",
                "This will update all supported objects to use the default teleportation layers.\n" +
                "This will overwrite raycast and interaction layer masks on supported interactors and interactables.\n" +
                "Proceed?", "Yes", "No"))
            {
                foreach (GameObject configuratorGameObject in configuratorGameObjects)
                {
                    ILayerConfigurator configurator = configuratorGameObject.GetComponent<ILayerConfigurator>();
                    if (configurator.LayerSet == LayerSet.Teleportation)
                    {
                        Debug.Log($"Configuring teleportation layers on '{configuratorGameObject.name}'.");
                        configurator.ConfigureLayers(teleportInteractionLayer, teleportRaycastLayer);
                        EditorUtility.SetDirty(configuratorGameObject);
                    }
                }
            }
        }
    }
}
