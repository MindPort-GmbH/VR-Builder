#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VRBuilder.Unity
{
    /// <summary>
    /// Utility class for prefabs.
    /// </summary>
    /// <remarks>
    /// This is not in the namespace <see cref="VRBuilder.Core.Editor"/> because <see cref="VRBuilder.Core"/> has no knowledge of <see cref="VRBuilder.Core.Editor"/>.
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
            if (component == null || component.gameObject == null)
            {
                // Component or GamObject is destroyed
                return false;
            }
            bool isSceneComponent = !IsOnDisk(component);
            if (isSceneComponent)
            {
                isSceneComponent = !IsEditingInPrefabMode(component.gameObject);
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

        /// <summary>
        /// Checks if the current object is in preview context.
        /// </summary>
        /// <returns><c>true</c> if the object is in preview context; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// The object is in preview context if it is <seealso cref="AssetUtility.IsOnDisk"/> or if it is being <seealso cref="AssetUtility.IsEditingInPrefabMode"/>. 
        /// </remarks>
        public static bool IsInPreviewContext(GameObject gameObject)
        {
            return IsOnDisk(gameObject) || IsEditingInPrefabMode(gameObject);
        }
    }
}
#endif
