using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
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
            return new SetComponentEnabledBehavior(CreateTargetObject(), "BoxCollider", false, false);
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
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given a behavior,
            ISceneObject targetObject = CreateTargetObject();
            Component component = targetObject.GameObject.AddComponent<AudioSource>();

            IBehavior behavior = new SetComponentEnabledBehavior(targetObject, "AudioSource", false, false);

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

            IBehavior behavior = new SetComponentEnabledBehavior(targetObject, "AudioSource", false, false);

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

            IBehavior behavior = new SetComponentEnabledBehavior(targetObject, "AudioSource", false, false);

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

            IBehavior behavior = new SetComponentEnabledBehavior(targetObject, "AudioSource", false, false);

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