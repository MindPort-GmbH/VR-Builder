#if VR_BUILDER_XR_INTERACTION
using System.Collections.Generic;
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
        internal static readonly Dictionary<int, string> DefaultInteractionLayers = new Dictionary<int, string>()
        {
            {31, "Teleport" }
        };

        static CreateDefaultInteractionLayers()
        {
            foreach (int index in DefaultInteractionLayers.Keys)
            {
                if (InteractionLayerUtils.TryAddLayerAtPosition(DefaultInteractionLayers[index], index) == false)
                {
                    Debug.LogError($"Interaction layer '{DefaultInteractionLayers[index]}' is not present and it was not possible to add it automatically.");
                }
            }
        }
    }
}
#endif