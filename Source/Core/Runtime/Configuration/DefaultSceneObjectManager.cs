using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Default single-user implementation of <see cref="ISceneObjectManager"/>.
    /// </summary>
    public class DefaultSceneObjectManager : ISceneObjectManager
    {
        /// <inheritdoc/>
        public void SetSceneObjectActive(ISceneObject sceneObject, bool isActive)
        {
            sceneObject.GameObject.SetActive(isActive);
        }

        /// <inheritdoc/>
        public void SetComponentActive(ISceneObject sceneObject, string componentTypeName, bool isActive)
        {
            IEnumerable<Component> components = sceneObject.GameObject.GetComponents<Component>().Where(c => c.GetType().Name == componentTypeName);

            foreach (Component component in components)
            {
                Type componentType = component.GetType();

                if (componentType.GetProperty("enabled") != null)
                {
                    componentType.GetProperty("enabled").SetValue(component, isActive, null);
                }
            }
        }

        /// <inheritdoc/>
        [Obsolete("Use InstantiateResourcePrefab instead. This method will be removed in a future version.")]
        public GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return GameObject.Instantiate(prefab, position, rotation);
        }

        /// <inheritdoc/>
        public T InstantiateResourcePrefab<T>(string resourcePath, Vector3 position, Quaternion rotation) where T : UnityEngine.Object
        {
            T prefab = Resources.Load<T>(resourcePath);

            if (prefab == null)
            {
                Debug.LogError($"Prefab not found at path: {resourcePath}");
                return null;
            }

            return GameObject.Instantiate<T>(prefab, position, rotation);
        }

        /// <inheritdoc/>
        public void RequestAuthority(ISceneObject sceneObject)
        {
        }
    }
}
