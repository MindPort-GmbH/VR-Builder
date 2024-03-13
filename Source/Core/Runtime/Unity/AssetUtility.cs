#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VRBuilder.Unity
{
    /// <summary>
    /// Utility class for prefabs.
    /// </summary>
    /// <remarks>
    /// This is not in the namespace <see cref="VRBuilder.Editor"/> because <see cref="VRBuilder.Core"/> has no knowledge of <see cref="VRBuilder.Editor"/>.
    /// At the current state <see cref="VRBuilder.Core.ProcessSceneObject"/> needs <see cref="IsOnDisk"/> .
    /// </remarks>
    public static class AssetUtility
    {

        /// <summary>
        /// Checks if the specified component is part of a scene game object
        /// </summary>
        /// <param name="component">The component to check.</param>
        /// <returns><c>true</c> if the component is in the scene; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// A component is not part of a scene game object if you are in prefab edit mode and or looking at a prefab in the project view.
        /// </remarks>
        public static bool IsComponentInScene(UnityEngine.Component component)
        {
            bool isSceneComponent = !IsOnDisk(component);
            if (isSceneComponent)
            {
                try
                {
                    isSceneComponent = !IsEditingInPrefabMode(component.gameObject);
                }
                catch (NullReferenceException)
                {
                    // Can happen when called from OnBeforeSerialize in prefab edit mode when adding or removing components or game objects
                    // TODO: Find a better way to handle OnBeforeSerialize calls
                    isSceneComponent = true;
                }
            }

            return isSceneComponent;

        }

        /// <summary>
        /// Checks if the gameObject is saved on disk.
        /// </summary>
        /// <returns><c>true</c> if the gameObject is saved on disk; otherwise, <c>false</c>.</returns>
        public static bool IsOnDisk(UnityEngine.Object asset)
        {
            // Happens when in prefab mode and adding or removing components
            if (asset == null)
            {
                return false;
            }

            if (PrefabUtility.IsPartOfPrefabAsset(asset))
            {
                return true;
            }

            if (EditorUtility.IsPersistent(asset))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the current object is being edited in prefab mode.
        /// </summary>
        /// <returns><c>true</c> if the object is being edited in prefab mode; otherwise, <c>false</c>.</returns>
        public static bool IsEditingInPrefabMode(GameObject gameObject)
        {
            var mainStage = StageUtility.GetMainStageHandle();
            var currentStage = StageUtility.GetStageHandle(gameObject);
            if (currentStage != mainStage)
            {
                var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
                if (prefabStage != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif