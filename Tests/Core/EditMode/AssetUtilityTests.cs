using NUnit.Framework;
using UnityEngine;
using VRBuilder.Core.Tests.Utils;
using VRBuilder.Unity;

namespace VRBuilder.Core.Tests
{
    public class AssetUtilityTests
    {
        private const string prefabPath = "Assets/TestPrefabsFolder_DeleteMe";

        [OneTimeSetUp]
        public void CreateTestPrefabFolder()
        {
            TestingUtils.CreateFolderRecursively(prefabPath);
        }

        [OneTimeTearDown]
        public void DeleteTestPrefabFolder()
        {
            TestingUtils.DeleteContentAndFolder(prefabPath);
        }

        [Test]
        public void IsComponentInScene_ComponentAttachedToSceneObject()
        {
            // Arrange
            GameObject gameObject = new GameObject("TestSceneObject");
            Rigidbody component = gameObject.AddComponent<Rigidbody>();

            // Act
            bool result = AssetUtility.IsComponentInScene(component);

            // Assert
            Assert.IsTrue(result);

            // Cleanup
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void IsComponentInScene_ComponentAttachedToPrefab()
        {
            // Arrange
            string prefabName = TestingUtils.GetUniqueName();
            GameObject go = new GameObject(prefabName);
            go.AddComponent<Rigidbody>();
            GameObject prefab = TestingUtils.CreatePrefab(go, prefabPath);

            // Act
            bool result = AssetUtility.IsComponentInScene(prefab.gameObject.GetComponent<Rigidbody>());

            // Assert
            Assert.IsFalse(result);

            // Cleanup
            TestingUtils.DeletePrefab(prefabName, prefabPath);
        }

        [Test]
        public void IsComponentInScene_Null()
        {
            // Act
            bool result = AssetUtility.IsComponentInScene(null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsOnDisk_SceneObject()
        {
            // Arrange
            GameObject gameObject = new GameObject("TestSceneObjectDisk");

            // Act
            bool result = AssetUtility.IsOnDisk(gameObject);

            // Assert
            Assert.IsFalse(result);

            // Cleanup
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void IsOnDisk_PrefabOnDisk()
        {
            // Arrange
            string prefabName = TestingUtils.GetUniqueName();
            GameObject go = new GameObject(prefabName);
            GameObject prefab = TestingUtils.CreatePrefab(go, prefabPath);

            // Act
            bool result = AssetUtility.IsOnDisk(prefab);

            // Assert
            Assert.IsTrue(result);

            // Cleanup
            TestingUtils.DeletePrefab(prefabName, prefabPath);
        }

        [Test]
        public void IsOnDisk_InstantiatedPrefab()
        {
            // Arrange
            string prefabName = TestingUtils.GetUniqueName();
            GameObject go = new GameObject(prefabName);
            GameObject prefab = TestingUtils.CreatePrefab(go, prefabPath);
            GameObject instantiatedPrefab = Object.Instantiate(prefab);

            // Act
            bool result = AssetUtility.IsOnDisk(instantiatedPrefab);

            // Assert
            Assert.IsFalse(result);

            // Cleanup
            TestingUtils.DeletePrefab(prefabName, prefabPath);
        }

        [Test]
        public void IsOnDisk_Null()
        {
            // Act
            bool result = AssetUtility.IsOnDisk(null);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
