#if VR_BUILDER_XR_INTERACTION
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRBuilder.Editor.XRInteractionExtension
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
        public static bool AddLayerIfNotPresent(string layerName, bool showDialog = false)
        {
            string dialogTitle = "Add interaction layer?";
            string dialogText = $"The required interaction layer '{layerName}' has not been found. Do you want to create it at the first available position?";
            string dialogConfirm = "Yes";
            string dialogCancel = "No";

            if (InteractionLayerSettings.Instance.GetLayer(layerName) > 0)
            {
                return true;
            }

            if (showDialog == false || EditorUtility.DisplayDialog(dialogTitle, dialogText, dialogConfirm, dialogCancel))
            {
                return AddLayer(layerName);
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
    }
}
#endif