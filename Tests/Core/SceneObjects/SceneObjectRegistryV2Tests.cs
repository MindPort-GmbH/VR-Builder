using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Tests.Utils;
using Object = UnityEngine.Object;

namespace VRBuilder.Tests
{
    public class SceneObjectRegistryV2Tests : RuntimeTests
    {
        [UnityTest]
        public IEnumerator IsRegisteredAtRegistryTest()
        {
            // Create reference
            GameObject obj = new GameObject("MyObject");
            ProcessSceneObject reference = obj.AddComponent<ProcessSceneObject>();

            // Await end of frame
            yield return new WaitForFixedUpdate();

            // Assert that reference is now registered at the registry.
            Assert.IsTrue(RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(reference.Guid));
            Assert.AreEqual(reference, RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(reference.Guid).First());
            Assert.AreEqual(1, RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(reference.Guid).Count());

            // Clean up
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        public IEnumerator CanBeFoundByUniqueIdTest()
        {
            // Create reference
            GameObject obj = new GameObject("MyObject");
            ProcessSceneObject reference = obj.AddComponent<ProcessSceneObject>();
            Guid guid = reference.Guid;

            // Await end of frame
            yield return new WaitForFixedUpdate();

            // Assert that reference is now registered at the registry.
            Assert.IsTrue(RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(guid));
            Assert.AreEqual(reference, RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid).First());
            Assert.AreEqual(1, RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid).Count());

            // Clean up
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        public IEnumerator CanBeFoundByTagTest()
        {
            // Given a tagged object,
            Guid tag = Guid.NewGuid();
            SceneObjectTags.Instance.CreateTag("test tag, delete me immediately", tag);
            GameObject obj1 = new GameObject("Test1");
            GameObject obj2 = new GameObject("Test2");
            ProcessSceneObject pso1 = obj1.AddComponent<ProcessSceneObject>();
            ProcessSceneObject pso2 = obj2.AddComponent<ProcessSceneObject>();
            pso1.AddTag(tag);
            pso2.AddTag(tag);

            // Await end of frame
            yield return new WaitForFixedUpdate();

            // Assert that it is found by tag in the registry
            Assert.IsTrue(RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(tag));
            Assert.AreEqual(2, RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(tag).Count());
            Assert.IsTrue(RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(tag).Contains(pso1));
            Assert.IsTrue(RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(tag).Contains(pso2));

            // Clean up
            Object.DestroyImmediate(obj1);
            Object.DestroyImmediate(obj2);
            SceneObjectTags.Instance.RemoveTag(tag);
        }

        [UnityTest]
        public IEnumerator UnregisterRemovesReferenceTest()
        {
            // Create reference
            GameObject obj = new GameObject("MyObject");
            ProcessSceneObject reference = obj.AddComponent<ProcessSceneObject>();
            Guid guid = reference.Guid;

            // Await end of frame
            yield return new WaitForFixedUpdate();

            RuntimeConfigurator.Configuration.SceneObjectRegistry.Unregister(reference);

            // Assert that you cant find reference by guid or name
            Assert.IsFalse(RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(guid));

            // Clean up
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        public IEnumerator UnregisterOnDestroyTest()
        {
            // Create reference
            GameObject obj = new GameObject("MyObject");
            ProcessSceneObject reference = obj.AddComponent<ProcessSceneObject>();
            Guid guid = reference.Guid;

            // Await end of frame
            yield return new WaitForFixedUpdate();

            Object.DestroyImmediate(obj);

            // Assert that object is not registered anymore
            Assert.IsFalse(RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(guid));

            yield return null;
        }

        [UnityTest]
        public IEnumerator ObjectCopyGetsUniqueId()
        {
            // Create object
            GameObject obj = new GameObject("MyObject");
            ProcessSceneObject reference = obj.AddComponent<ProcessSceneObject>();
            Guid guid = reference.Guid;

            GameObject copy = GameObject.Instantiate(obj);
            ProcessSceneObject copyReference = copy.GetComponent<ProcessSceneObject>();
            Guid copyGuid = copyReference.Guid;

            // Wait for end of frame
            yield return new WaitForFixedUpdate();

            // Assert that objects have different GUIDs
            Assert.IsTrue(guid != copyGuid);
            Assert.IsTrue(RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid).Count() == 1);
            Assert.IsTrue(RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(copyGuid).Count() == 1);
            Assert.AreEqual(reference, RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid).First());
            Assert.AreEqual(copyReference, RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(copyGuid).First());

            Object.DestroyImmediate(obj);
            Object.DestroyImmediate(copy);
        }

        [UnityTest]
        public IEnumerator ChangingNameUpdatesRegistration()
        {
            // Create object
            GameObject obj = new GameObject("MyObject");
            ProcessSceneObject reference = obj.AddComponent<ProcessSceneObject>();
            Guid oldGuid = reference.Guid;
            Guid newGuid = Guid.NewGuid();

            // Wait for end of frame
            yield return new WaitForFixedUpdate();

            reference.ChangeUniqueId(newGuid);

            yield return new WaitForFixedUpdate();

            Assert.AreNotEqual(oldGuid, newGuid);
            Assert.IsFalse(RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(oldGuid));
            Assert.IsTrue(RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(newGuid));
            Assert.AreEqual(1, RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(newGuid).Count());
            Assert.AreEqual(reference, RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(newGuid).First());

            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        public IEnumerator RemovingLastUsedTagRemovesGuidFromRegistry()
        {
            // Given a tagged object,
            Guid tag = Guid.NewGuid();
            SceneObjectTags.Instance.CreateTag("test tag, delete me immediately", tag);
            GameObject obj1 = new GameObject("Test1");
            ProcessSceneObject processSceneObject = obj1.AddComponent<ProcessSceneObject>();
            processSceneObject.AddTag(tag);

            // Await end of frame
            yield return new WaitForFixedUpdate();

            processSceneObject.RemoveTag(tag);

            // Assert that the tag guid does not exist in the registry.
            Assert.IsFalse(RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(tag));
            Assert.IsTrue(RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(processSceneObject.Guid));

            // Clean up
            Object.DestroyImmediate(obj1);
            SceneObjectTags.Instance.RemoveTag(tag);
        }
    }
}