using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.TestTools;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Tests.Utils;

namespace VRBuilder.Core.Tests.Behaviors
{
    [TestFixture]
    public class SetComponentEnabledBehaviorTests : BehaviorTests
    {
        List<GameObject> spawnedObjects = new List<GameObject>();

        protected override IBehavior CreateDefaultBehavior()
        {
            return new SetComponentEnabledByTagBehavior(CreateTargetObject().Guid, "BoxCollider", false, false);
        }

        protected ISceneObject CreateTargetObject(string name = "Target Object")
        {
            GameObject targetObject = new GameObject("Target Object");
            targetObject.AddComponent<BoxCollider>();
            targetObject.AddComponent<AimConstraint>();
            spawnedObjects.Add(targetObject);

            return targetObject.AddComponent<ProcessSceneObject>();
        }

        [TearDown]
        public void DeleteAllObjects()
        {
            foreach (GameObject spawnedObject in spawnedObjects)
            {
                GameObject.DestroyImmediate(spawnedObject);
            }

            spawnedObjects.Clear();
        }

        [UnityTest]
        public IEnumerator CreateByReference()
        {
            // Given the necessary parameters,
            ISceneObject targetObject = CreateTargetObject();
            string componentType = "BoxCollider";
            bool enable = true;
            bool revert = true;

            // When we create the behavior passing process objects by reference,
            SetComponentEnabledByTagBehavior behavior = new SetComponentEnabledByTagBehavior(targetObject.Guid, componentType, enable, revert);

            // Then all properties of the behavior are properly assigned.
            Assert.AreEqual(targetObject, behavior.Data.TargetObjects.Values.First());
            Assert.AreEqual(componentType, behavior.Data.ComponentType);
            Assert.AreEqual(enable, behavior.Data.SetEnabled);
            Assert.AreEqual(revert, behavior.Data.RevertOnDeactivation);

            yield break;
        }

        [UnityTest]
        public IEnumerator ComponentsAreDisabled()
        {
            // Given a behavior,
            ISceneObject spawnedObject = CreateTargetObject();
            spawnedObject.GameObject.AddComponent<BoxCollider>();

            IBehavior behavior = new SetComponentEnabledByTagBehavior(spawnedObject.Guid, "BoxCollider", false, false);

            // When it is activated,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            // Then the target components are disabled.
            Assert.IsTrue(spawnedObject.GameObject.GetComponents<BoxCollider>().Length > 0);
            foreach (BoxCollider collider in spawnedObject.GameObject.GetComponents<BoxCollider>())
            {
                Assert.IsFalse(collider.enabled);
            }
        }

        [UnityTest]
        public IEnumerator NoComponentsAreDisabled()
        {
            // Given a behavior,
            ISceneObject spawnedObject = CreateTargetObject();
            spawnedObject.GameObject.AddComponent<BoxCollider>();

            IBehavior behavior = new SetComponentEnabledByTagBehavior(spawnedObject.Guid, "", false, false);

            // When it is activated,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            // Then the target components are disabled.
            Assert.IsTrue(spawnedObject.GameObject.GetComponents<BoxCollider>().Length > 0);
            foreach (BoxCollider collider in spawnedObject.GameObject.GetComponents<BoxCollider>())
            {
                Assert.IsTrue(collider.enabled);
            }
            Assert.IsTrue(spawnedObject.GameObject.GetComponent<AimConstraint>().enabled);
        }

        [UnityTest]
        public IEnumerator ComponentsAreEnabled()
        {
            // Given a behavior,
            ISceneObject spawnedObject = CreateTargetObject();
            spawnedObject.GameObject.AddComponent<BoxCollider>();

            spawnedObject.GameObject.GetComponents<BoxCollider>().ToList().ForEach(c => c.enabled = false);

            IBehavior behavior = new SetComponentEnabledByTagBehavior(spawnedObject.Guid, "BoxCollider", true, false);

            // When it is activated,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            // Then the target components are disabled.
            Assert.IsTrue(spawnedObject.GameObject.GetComponents<BoxCollider>().Length > 0);
            foreach (BoxCollider collider in spawnedObject.GameObject.GetComponents<BoxCollider>())
            {
                Assert.IsTrue(collider.enabled);
            }
        }

        [UnityTest]
        public IEnumerator ComponentsAreDisabledThenEnabled()
        {
            // Given a behavior,
            ISceneObject spawnedObject = CreateTargetObject();
            spawnedObject.GameObject.AddComponent<BoxCollider>();

            IBehavior behavior = new SetComponentEnabledByTagBehavior(spawnedObject.Guid, "BoxCollider", false, true);

            // When it is activated,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            bool wasDisabled = false;
            foreach (BoxCollider collider in spawnedObject.GameObject.GetComponents<BoxCollider>())
            {
                wasDisabled |= collider.enabled;
            }

            behavior.LifeCycle.Deactivate();

            while (behavior.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                behavior.Update();
            }

            // Then the target components are disabled.
            Assert.IsTrue(spawnedObject.GameObject.GetComponents<BoxCollider>().Length > 0);
            foreach (BoxCollider collider in spawnedObject.GameObject.GetComponents<BoxCollider>())
            {
                Assert.IsTrue(collider.enabled);
            }
            Assert.IsFalse(wasDisabled);
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given a behavior,
            ISceneObject targetObject = CreateTargetObject();
            Component component = targetObject.GameObject.AddComponent<AudioSource>();

            IBehavior behavior = new SetComponentEnabledByTagBehavior(targetObject.Guid, "AudioSource", false, false);

            // When we mark it to fast-forward and activate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            // Then it autocompletes immediately.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsFalse(targetObject.GameObject.GetComponent<AudioSource>().enabled);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndDeactivateIt()
        {
            // Given a behavior,
            ISceneObject targetObject = CreateTargetObject();
            Component component = targetObject.GameObject.AddComponent<AudioSource>();

            IBehavior behavior = new SetComponentEnabledByTagBehavior(targetObject.Guid, "AudioSource", false, false);

            // When we mark it to fast-forward, activate and immediately deactivate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            behavior.LifeCycle.Deactivate();

            // Then it autocompletes immediately.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
            Assert.IsFalse(targetObject.GameObject.GetComponent<AudioSource>().enabled);
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBehavior()
        {
            // Given a behavior,
            ISceneObject targetObject = CreateTargetObject();
            Component component = targetObject.GameObject.AddComponent<AudioSource>();

            IBehavior behavior = new SetComponentEnabledByTagBehavior(targetObject.Guid, "AudioSource", false, false);

            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it autocompletes immediately.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsFalse(targetObject.GameObject.GetComponent<AudioSource>().enabled);
        }

        [UnityTest]
        public IEnumerator FastForwardDeactivatingBehavior()
        {
            // Given a behavior,
            ISceneObject targetObject = CreateTargetObject();
            Component component = targetObject.GameObject.AddComponent<AudioSource>();

            IBehavior behavior = new SetComponentEnabledByTagBehavior(targetObject.Guid, "AudioSource", false, false);

            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            behavior.LifeCycle.Deactivate();

            while (behavior.LifeCycle.Stage != Stage.Deactivating)
            {
                yield return null;
                behavior.Update();
            }

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it autocompletes immediately.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
            Assert.IsFalse(targetObject.GameObject.GetComponent<AudioSource>().enabled);
        }
    }
}