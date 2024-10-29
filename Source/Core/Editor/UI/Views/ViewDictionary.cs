using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.Views
{
    /// <summary>
    /// A static class that holds the path to UXML files mapped by an enum containing the UXML file names 
    /// </summary>
    public class ViewDictionary
    {
        // Dictionary to hold the mapping between enum values and UXML file paths
        private static Dictionary<EnumType, string> uxmlPaths = new Dictionary<EnumType, string>()
        {
            { EnumType.SearchableList, $"{EditorUtils.GetCoreFolder()}/Source/Core/Editor/UI/Views/SearchableList.uxml" },
            { EnumType.GroupListItem, $"{EditorUtils.GetCoreFolder()}/Source/Core/Editor/UI/Views/GroupListItem.uxml" },
            { EnumType.SceneReferencesList, $"{EditorUtils.GetCoreFolder()}/Source/Core/Editor/UI/Views/SceneReferencesList.uxml" },
            { EnumType.SceneReferencesGroupItem, $"{EditorUtils.GetCoreFolder()}/Source/Core/Editor/UI/Views/SceneReferencesGroupItem.uxml" },
            { EnumType.SceneReferencesObjectItem, $"{EditorUtils.GetCoreFolder()}/Source/Core/Editor/UI/Views/SceneReferencesObjectItem.uxml" }
        };

        // Enum containing the UXML file names
        public enum EnumType
        {
            SearchableList,
            GroupListItem,
            SceneReferencesList,
            SceneReferencesGroupItem,
            SceneReferencesObjectItem
        }

        /// <summary>
        /// Retrieves the path associated with the specified enum value.
        /// </summary>
        /// <param name="enumValue">The enum value to retrieve the path for.</param>
        /// <returns>The path associated with the enum value, or null if the path is not found.</returns>
        public static string GetPath(EnumType enumValue)
        {
            if (uxmlPaths.ContainsKey(enumValue))
            {
                return uxmlPaths[enumValue];
            }
            else
            {
                UnityEngine.Debug.LogError("Path not found for enum value: " + enumValue);
                return null;
            }
        }

        /// <summary>
        /// Loads a VisualTreeAsset based on the provided enum value.
        /// </summary>
        /// <param name="enumValue">The enum value representing the desired asset.</param>
        /// <returns>The loaded VisualTreeAsset, or null if the asset could not be found.</returns>
        public static VisualTreeAsset LoadAsset(EnumType enumValue)
        {
            string path = GetPath(enumValue);
            if (path != null)
            {
                return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            }
            else
            {
                return null;
            }
        }
    }
}
