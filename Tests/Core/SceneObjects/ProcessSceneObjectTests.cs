using NUnit.Framework;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
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

        [UnityTest]
        public IEnumerator DuplicatedObjectsHaveDifferentUniqueIds()
        {
            // Given a PSO in the scene,
            ProcessSceneObject original = GameObject.Instantiate(new GameObject("Test")).AddComponent<ProcessSceneObject>();
            Guid originalGuid = GetSerializedGuid(original).Guid;

            // When I duplicate it by re-instantiating it,
            ProcessSceneObject copy = GameObject.Instantiate(original) as ProcessSceneObject;
            Guid copyGuid = GetSerializedGuid(copy).Guid;

            // Then the new object has a different unique id.
            Assert.AreNotEqual(Guid.Empty, originalGuid);
            Assert.AreNotEqual(Guid.Empty, copyGuid);
            Assert.AreNotEqual(originalGuid, copyGuid);

            // Cleanup
            GameObject.DestroyImmediate(original);
            GameObject.DestroyImmediate(copy);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PrefabHasNoUniqueId()
        {
            // Given a PSO,
            string prefabName = GetUniquePrefabName();
            ProcessSceneObject processSceneObject = new GameObject(prefabName).AddComponent<ProcessSceneObject>();
            Guid uniqueId = GetSerializedGuid(processSceneObject).Guid;

            // When I save it as a prefab,
            GameObject prefab = CreatePrefab(processSceneObject.gameObject);

            // Then it does not retain its unique id.
            Assert.AreNotEqual(Guid.Empty, uniqueId);
            Assert.IsNull(GetSerializedGuid(prefab.GetComponent<ProcessSceneObject>()));

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
            Guid guid1 = GetSerializedGuid(spawnedObject1).Guid;
            Guid guid2 = GetSerializedGuid(spawnedObject2).Guid;

            Assert.AreNotEqual(Guid.Empty, guid1);
            Assert.AreNotEqual(Guid.Empty, guid2);
            Assert.AreNotEqual(guid1, guid2);

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

            // When I modify it and then apply the changes,
            instance.AddComponent<Rigidbody>();
            PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);

            // Then the unique id on the original prefab is not overwritten.
            Assert.IsNotNull(prefab.GetComponent<Rigidbody>());
            Assert.IsTrue(GetSerializedGuid(instance.GetComponent<ProcessSceneObject>()).IsValid());
            Assert.IsFalse(GetSerializedGuid(prefab.GetComponent<ProcessSceneObject>()).IsValid());

            // Cleanup
            yield return null;
            DeletePrefab(prefabName);
            GameObject.DestroyImmediate(instance);
        }

        [UnityTest]
        public IEnumerator ResettingInstanceChangesDoesNotChangeGuid()
        {
            // Given an instance of a PSO prefab,
            string prefabName = GetUniquePrefabName();
            GameObject prefab = CreateProcessSceneObjectPrefab(prefabName);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Guid instanceGuid = GetSerializedGuid(instance.GetComponent<ProcessSceneObject>()).Guid;

            // When I modify it and then reset the changes,
            instance.AddComponent<Rigidbody>();
            PrefabUtility.RevertPrefabInstance(instance, InteractionMode.AutomatedAction);

            // The unique id of the instance stays the same.
            Assert.IsNull(instance.GetComponent<Rigidbody>());
            Assert.AreEqual(instanceGuid, GetSerializedGuid(instance.GetComponent<ProcessSceneObject>()).Guid);

            // Cleanup
            yield return null;
            DeletePrefab(prefabName);
            GameObject.DestroyImmediate(instance);
        }

        [UnityTest]
        public IEnumerator WhenAddingToPrefabProcessSceneObjectInstancesObtainDifferentGuids()
        {
            // Given a prefab of which multiple instances are present in the scene,
            string prefabName = GetUniquePrefabName();
            GameObject prefab = CreatePrefab(new GameObject(prefabName));
            GameObject instance1 = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            GameObject instance2 = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            // When I add a process scene object to it,
            prefab.AddComponent<ProcessSceneObject>();
            PrefabUtility.SavePrefabAsset(prefab);

            // Then all instances of the prefab have a different unique id.
            SerializableGuid guid1 = GetSerializedGuid(instance1.GetComponent<ProcessSceneObject>());
            SerializableGuid guid2 = GetSerializedGuid(instance2.GetComponent<ProcessSceneObject>());

            Assert.IsTrue(guid1.IsValid());
            Assert.IsTrue(guid2.IsValid());
            Assert.AreNotEqual(guid1, guid2);

            // Cleanup
            yield return null;
            DeletePrefab(prefabName);
            GameObject.DestroyImmediate(instance1);
            GameObject.DestroyImmediate(instance2);
        }

        [UnityTest]
        public IEnumerator ApplyTag()
        {
            // Given an instance of a PSO prefab,
            string prefabName = GetUniquePrefabName();
            GameObject prefab = CreateProcessSceneObjectPrefab(prefabName);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Guid tag = Guid.NewGuid();

            // When I modify the tag and then apply the changes,
            ProcessSceneObject pso = instance.GetComponent<ProcessSceneObject>();
            pso.AddTag(tag);
            PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);

            // Then the unique id on the original prefab is not overwritten the tags are applied
            Assert.IsTrue(GetSerializedGuid(instance.GetComponent<ProcessSceneObject>()).IsValid());
            Assert.IsFalse(GetSerializedGuid(prefab.GetComponent<ProcessSceneObject>()).IsValid());
            Assert.IsTrue(instance.GetComponent<ProcessSceneObject>().HasTag(tag));
            Assert.IsTrue(prefab.GetComponent<ProcessSceneObject>().HasTag(tag));

            // Cleanup
            yield return null;
            DeletePrefab(prefabName);
            GameObject.DestroyImmediate(instance);
        }

        [UnityTest]
        public IEnumerator RevertTag()
        {
            // Given an instance of a PSO prefab with a Tag,
            string prefabName = GetUniquePrefabName();
            GameObject prefab = CreateProcessSceneObjectPrefab(prefabName);
            ProcessSceneObject prefabPso = prefab.GetComponent<ProcessSceneObject>();
            Guid originalTag = Guid.NewGuid();
            prefabPso.AddTag(originalTag);

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            ProcessSceneObject instancePso = instance.GetComponent<ProcessSceneObject>();

            // When I add a second the tag and then revert the changes,
            Guid newTag = Guid.NewGuid();
            instancePso.AddTag(newTag);
            Guid instanceGuid = GetSerializedGuid(instancePso).Guid;
            PrefabUtility.RevertPrefabInstance(instance, InteractionMode.AutomatedAction);

            // The unique id of the instance stays the same but the tags are reverted    
            Assert.AreEqual(instanceGuid, GetSerializedGuid(instance.GetComponent<ProcessSceneObject>()).Guid);
            Assert.IsTrue(instancePso.HasTag(originalTag));
            Assert.IsTrue(prefabPso.HasTag(originalTag));
            Assert.IsFalse(instancePso.HasTag(newTag));

            // Cleanup
            yield return null;
            DeletePrefab(prefabName);
            GameObject.DestroyImmediate(instance);
        }

        private SerializableGuid GetSerializedGuid(ProcessSceneObject processSceneObject)
        {
            Type processSceneObjectType = typeof(ProcessSceneObject);
            FieldInfo serializableGuidField = processSceneObjectType.GetField("serializedGuid", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.ExactBinding);

            if (serializableGuidField != null)
            {
                return (SerializableGuid)serializableGuidField.GetValue(processSceneObject);
            }
            else
            {
                throw new MissingFieldException("Field 'serializedGuid' not found in class 'ProcessSceneObject'.");
            }
        }

        private void CreateFolderRecursively(string path)
        {
            string parentPath = Path.GetDirectoryName(path);
            if (!AssetDatabase.IsValidFolder(parentPath))
            {
                CreateFolderRecursively(parentPath);
            }

            AssetDatabase.CreateFolder(parentPath, Path.GetFileName(path));
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
    }

}