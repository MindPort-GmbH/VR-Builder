#if VR_BUILDER_XR_INTERACTION
using UnityEditor;
using UnityEngine;
using VRBuilder.Editor.XRInteractionExtension;

namespace VRBuilder.Editor.XRInteraction
{
    /// <summary>
    /// Automatically creates the required interaction layers if these are not present.
    /// </summary>
    [InitializeOnLoad]
    internal static class CreateDefaultInteractionLayers
    {
        /// <summary>
        /// List of interaction layers that VR Builder creates automatically.
        /// </summary>
        internal static string[] DefaultInteractionLayers =
        {
            "XR Teleport",
        };

        static CreateDefaultInteractionLayers()
        {
            foreach (string layer in DefaultInteractionLayers)
            {
                if (InteractionLayerUtils.AddLayerIfNotPresent(layer) == false)
                {
                    Debug.LogError($"Interaction layer '{layer}' is not present and it was not possible to add it automatically.");
                }
            }
        }
    }
}
#endif