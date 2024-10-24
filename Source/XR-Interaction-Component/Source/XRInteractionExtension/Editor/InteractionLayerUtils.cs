#if VR_BUILDER_XR_INTERACTION
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRBuilder.XRInteraction.EditorExtension
{
    /// <summary>
    /// Utilities to manipulate XRI interaction layers.
    /// </summary>
    public static class InteractionLayerUtils
    {
        /// <summary>
        /// Adds an interaction layer with the specified name if not already present.
        /// </summary>
        /// <param name="layerName">Name of the layer to add.</param>
        /// <param name="showDialog">If true, a dialog will be shown for user confirmation.</param>
        /// <returns>True if the layer has been added or was already present.</returns>
        public static bool AddLayerIfNotPresent(string layerName, bool showDialog = false, int preferredPosition = -1)
        {
            string dialogTitle = "Add interaction layer?";
            string dialogText = $"The required interaction layer '{layerName}' has not been found. Do you want to create it?";
            string dialogConfirm = "Yes";
            string dialogCancel = "No";

            if (InteractionLayerSettings.Instance.GetLayer(layerName) > 0)
            {
                return true;
            }

            if (showDialog == false || EditorUtility.DisplayDialog(dialogTitle, dialogText, dialogConfirm, dialogCancel))
            {
                return TryAddLayerAtPosition(layerName, preferredPosition);
            }

            return false;
        }

        /// <summary>
        /// Adds an interaction layer with the specified name.
        /// </summary>
        /// <param name="layerName">Name of the layer to add.</param>
        /// <returns>True if the layer has been successfully added.</returns>
        public static bool AddLayer(string layerName)
        {
            const string layerNamesPropertyPath = "m_LayerNames";

            SerializedObject interactionLayerSettingsSo = new SerializedObject(InteractionLayerSettings.Instance);
            SerializedProperty layerNamesProperty = interactionLayerSettingsSo.FindProperty(layerNamesPropertyPath);

            // built-in Interaction Layer names are not editable, so they are ignored 
            for (int i = InteractionLayerSettings.builtInLayerSize; i < InteractionLayerSettings.layerSize; i++)
            {
                SerializedProperty interactionLayerNameProperty = layerNamesProperty.GetArrayElementAtIndex(i);

                if (interactionLayerNameProperty.stringValue == null || string.IsNullOrEmpty(interactionLayerNameProperty.stringValue))
                {
                    interactionLayerNameProperty.stringValue = layerName;
                    interactionLayerSettingsSo.ApplyModifiedProperties();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to add a layer at a specified position. If the position is already occupied,
        /// tries to add it on the first available spot.
        /// </summary>
        /// <param name="layerName">Name of the layer to add.</param>
        /// <param name="preferredPosition">Preferred index in the interaction layers array.</param>
        /// <returns>True if a layer was added.</returns>
        public static bool TryAddLayerAtPosition(string layerName, int preferredPosition)
        {
            const string layerNamesPropertyPath = "m_LayerNames";

            SerializedObject interactionLayerSettingsSo = new SerializedObject(InteractionLayerSettings.Instance);
            SerializedProperty layerNamesProperty = interactionLayerSettingsSo.FindProperty(layerNamesPropertyPath);

            if (preferredPosition >= InteractionLayerSettings.builtInLayerSize && preferredPosition < InteractionLayerSettings.layerSize)
            {
                SerializedProperty interactionLayerNameProperty = layerNamesProperty.GetArrayElementAtIndex(preferredPosition);

                if (interactionLayerNameProperty.stringValue == null || string.IsNullOrEmpty(interactionLayerNameProperty.stringValue))
                {
                    interactionLayerNameProperty.stringValue = layerName;
                    interactionLayerSettingsSo.ApplyModifiedProperties();
                    return true;
                }
            }

            return AddLayer(layerName);
        }
    }
}
#endif