// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Exceptions;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Tests.Utils;

namespace VRBuilder.Tests
{
    public class SceneObjectRegistryTests : RuntimeTests
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
            Assert.AreEqual(reference, RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByGuid(reference.Guid));

            // Clean up
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        public IEnumerator CanBeFoundByTagTest()
        {
            // Given a tagged object,
            System.Guid tag = System.Guid.NewGuid();
            SceneObjectTags.Instance.CreateTag("test tag, delete me immediately", tag);
            GameObject obj1 = new GameObject("Test1");
            ProcessSceneObject processSceneObject = obj1.AddComponent<ProcessSceneObject>();
            processSceneObject.AddTag(tag);

            // Await end of frame
            yield return new WaitForFixedUpdate();

            // Assert that it is found by tag in the registry
            Assert.AreEqual(1, RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(tag).Count());
            Assert.AreEqual(processSceneObject, RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(tag).First());

            // Clean up
            Object.DestroyImmediate(obj1);
            SceneObjectTags.Instance.RemoveTag(tag);
        }

        [UnityTest]
        public IEnumerator UnregisterRemovesReferenceTest()
        {
            // Create reference
            GameObject obj = new GameObject("MyObject");
            ProcessSceneObject reference = obj.AddComponent<ProcessSceneObject>();

            // Await end of frame
            yield return new WaitForFixedUpdate();

            RuntimeConfigurator.Configuration.SceneObjectRegistry.Unregister(reference);
            // Assert that you cant find reference by guid or name
            Assert.IsFalse(RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(reference.Guid));

            // Clean up
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        public IEnumerator UnregisterAllowsToPlaceTest()
        {
            // Create reference
            GameObject obj1 = new GameObject("MyObject");
            ProcessSceneObject reference1 = obj1.AddComponent<ProcessSceneObject>();

            RuntimeConfigurator.Configuration.SceneObjectRegistry.Unregister(reference1);

            // Create reference
            GameObject obj2 = new GameObject("MyObject");
            ProcessSceneObject reference2 = obj2.AddComponent<ProcessSceneObject>();

            // Assert that new added reference can be found
            Assert.IsTrue(RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(reference2.Guid));
            Assert.AreEqual(reference2, RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByGuid(reference2.Guid));

            // Clean up
            Object.DestroyImmediate(obj1);
            Object.DestroyImmediate(obj2);

            yield return null;
        }

        [UnityTest]
        public IEnumerator CantBeRegisteredTwiceTest()
        {
            // Create reference
            GameObject obj = new GameObject("MyObject");
            ProcessSceneObject reference = obj.AddComponent<ProcessSceneObject>();

            // Assert that exception is thrown
            Assert.Throws(typeof(AlreadyRegisteredException),
                () =>
                {
                    RuntimeConfigurator.Configuration.SceneObjectRegistry.Register(reference);
                },
                "ReferenceAlreadyRegisteredException was not thrown!");

            // Clean up
            Object.DestroyImmediate(obj);

            yield return null;
        }

        [UnityTest]
        public IEnumerator UnregisterOnDestroyTest()
        {
            // Create reference
            GameObject obj = new GameObject("MyObject");
            ProcessSceneObject reference = obj.AddComponent<ProcessSceneObject>();
            System.Guid guid = reference.Guid;

            Object.DestroyImmediate(obj);

            // Assert that exception is thrown
            Assert.IsFalse(RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(guid));

            yield return null;
        }
    }
}
