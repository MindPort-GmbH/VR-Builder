using NUnit.Framework;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Tests.Utils;

namespace VRBuilder.Tests
{
    public class ProcessSceneObjectTests : RuntimeTests
    {
        private const string prefabPath = "Assets/TestPrefabsFolder_DeleteMe";

        [OneTimeSetUp]
        public void CreateTestPrefabFolder()
        {
            if (AssetDatabase.IsValidFolder(prefabPath) == false)
            {
                CreateFolderRecursively(prefabPath);
            }
        }

        [OneTimeTearDown]
        public void DeleteTestPrefabFolder()
        {
            // Check if the folder exists
            if (AssetDatabase.IsValidFolder(prefabPath))
            {
                // Get all assets in the folder
                string[] assets = AssetDatabase.FindAssets("", new[] { prefabPath });

                // Delete each asset in the folder
                foreach (string assetGUID in assets)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                    AssetDatabase.DeleteAsset(assetPath);
                }

                // Delete the folder
                AssetDatabase.DeleteAsset(prefabPath);
            }
        }

        private void CreateFolderRecursively(string path)
        {
            string parentPath = System.IO.Path.GetDirectoryName(path);
            if (!AssetDatabase.IsValidFolder(parentPath))
            {
                CreateFolderRecursively(parentPath);
            }

            AssetDatabase.CreateFolder(parentPath, System.IO.Path.GetFileName(path));
        }

        private GameObject CreatePrefab(GameObject originalObject)
        {
            string prefabName = originalObject.name;
            PrefabUtility.SaveAsPrefabAsset(originalObject, $"{prefabPath}/{prefabName}.prefab");
            GameObject.DestroyImmediate(originalObject);
            return AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}/{prefabName}.prefab");
        }

        private GameObject CreateProcessSceneObjectPrefab(string prefabName)
        {
            GameObject processSceneObject = new GameObject(prefabName);
            processSceneObject.AddComponent<ProcessSceneObject>();
            return CreatePrefab(processSceneObject);
        }

        private string GetUniquePrefabName()
        {
            return $"TestPrefab-{Guid.NewGuid()}";
        }

        private void DeletePrefab(string prefabName)
        {
            AssetDatabase.DeleteAsset($"{prefabPath}/{prefabName}.prefab");
        }

        [UnityTest]
        public IEnumerator PrefabHasNoUniqueId()
        {
            // Given a PSO,
            string prefabName = GetUniquePrefabName();
            ProcessSceneObject processSceneObject = new GameObject(prefabName).AddComponent<ProcessSceneObject>();

            Guid uniqueId = processSceneObject.Guid;

            // When I save it as a prefab,
            GameObject loadedPrefab = CreatePrefab(processSceneObject.gameObject);

            // Then it does not retain its unique id.
            Assert.AreNotEqual(Guid.Empty, uniqueId);
            Assert.AreEqual(Guid.Empty, loadedPrefab.GetComponent<ProcessSceneObject>().Guid);

            // Cleanup
            DeletePrefab(prefabName);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DifferentSpawnedObjectsHaveDifferentUniqueIds()
        {
            // Given a PSO prefab,
            string prefabName = GetUniquePrefabName();
            GameObject processSceneObjectPrefab = CreateProcessSceneObjectPrefab(prefabName);

            // When I spawn it multiple times in the scene,
            ProcessSceneObject spawnedObject1 = ((GameObject)PrefabUtility.InstantiatePrefab(processSceneObjectPrefab)).GetComponent<ProcessSceneObject>();
            ProcessSceneObject spawnedObject2 = ((GameObject)PrefabUtility.InstantiatePrefab(processSceneObjectPrefab)).GetComponent<ProcessSceneObject>();

            // Then each spawned object has a different unique id.
            Assert.AreNotEqual(Guid.Empty, spawnedObject1.Guid);
            Assert.AreNotEqual(Guid.Empty, spawnedObject2.Guid);
            Assert.AreNotEqual(spawnedObject1.Guid, spawnedObject2.Guid);

            // Cleanup
            DeletePrefab(prefabName);
            GameObject.DestroyImmediate(spawnedObject1);
            GameObject.DestroyImmediate(spawnedObject2);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ApplyChangesDoesNotOverrideGuid()
        {
            // Given an instance of a PSO prefab,
            string prefabName = GetUniquePrefabName();
            GameObject prefab = CreateProcessSceneObjectPrefab(prefabName);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            AssetDatabase.Refresh();
            Guid uniqueId = instance.GetComponent<ProcessSceneObject>().Guid;

            // If I modify it and then apply the changes,
            instance.AddComponent<Rigidbody>();
            PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);

            // Then the unique id on the original prefab is not overwritten.
            Assert.IsNotNull(prefab.GetComponent<Rigidbody>());
            Assert.AreNotEqual(uniqueId, prefab.GetComponent<ProcessSceneObject>().Guid);
            Assert.AreEqual(Guid.Empty, prefab.GetComponent<ProcessSceneObject>().Guid);

            // Cleanup
            yield return null;
            DeletePrefab(prefabName);
            GameObject.DestroyImmediate(instance);
        }
    }
}