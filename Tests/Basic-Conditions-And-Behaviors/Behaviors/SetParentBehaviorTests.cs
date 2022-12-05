using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Tests.Utils;

namespace VRBuilder.Core.Tests.Behaviors
{
    [TestFixture]    
    public class SetParentBehaviorTests : BehaviorTests
    {
        List<GameObject> spawnedObjects = new List<GameObject>();

        protected override IBehavior CreateDefaultBehavior()
        {
            ISceneObject target = SpawnTestObject("Target", Vector3.zero, Quaternion.identity, Vector3.one);
            ISceneObject parent = SpawnTestObject("Parent", Vector3.zero, Quaternion.identity, Vector3.one);
            return new SetParentBehavior(target, parent);
        }

        public ProcessSceneObject SpawnTestObject(string name, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent = null)
        {
            GameObject spawnedObject = new GameObject(name);
            spawnedObject.transform.SetPositionAndRotation(position, rotation);
            spawnedObject.transform.localScale = scale;
            spawnedObjects.Add(spawnedObject);            
            return spawnedObject.AddComponent<ProcessSceneObject>();
        }

        private static TestCaseData[] snapTestCases = new TestCaseData[]
        {
            new TestCaseData(new Vector3(3, -2, 6), Quaternion.Euler(-5, 7, 43)).Returns(null),
            new TestCaseData(new Vector3(75, 2, 8), Quaternion.Euler(123, 65, 41)).Returns(null),
            new TestCaseData(new Vector3(0, 0, 0), Quaternion.Euler(45, 2, -12)).Returns(null),
            new TestCaseData(new Vector3(5, -6, 2), Quaternion.Euler(0, 0, 0)).Returns(null),
        };

        [TearDown]
        public void DeleteAllObjects()
        {
            foreach(GameObject spawnedObject in spawnedObjects)
            {
                GameObject.DestroyImmediate(spawnedObject);
            }

            spawnedObjects.Clear();
        }

        [UnityTest]
        public IEnumerator ObjectIsParented()
        {
            // Given a set parent behavior,
            ProcessSceneObject target = SpawnTestObject("Target", Vector3.zero, Quaternion.identity, Vector3.one);
            ProcessSceneObject parent = SpawnTestObject("Parent", Vector3.zero, Quaternion.identity, Vector3.one);
            IBehavior behavior = new SetParentBehavior(target, parent);

            // When the behavior completes,
            behavior.LifeCycle.Activate();

            while(behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            // Then the target object has been parented.
            Assert.AreEqual(parent.transform, target.transform.parent);
        }

        [UnityTest]        
        [TestCaseSource(nameof(snapTestCases))]
        public IEnumerator ObjectSnapsToParentIfSet(Vector3 parentPosition, Quaternion parentRotation)
        {
            // Given a set parent behavior,
            ProcessSceneObject target = SpawnTestObject("Target", Vector3.zero, Quaternion.identity, Vector3.one);
            ProcessSceneObject parent = SpawnTestObject("Parent", parentPosition, parentRotation, Vector3.one);
            IBehavior behavior = new SetParentBehavior(target, parent, true);

            // When the behavior completes,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            // Then the target object has been parented, and it snaps to the parent's position.
            Assert.AreEqual(parent.transform, target.transform.parent);            
            Assert.IsTrue((parent.transform.position - target.transform.position).sqrMagnitude < 0.001f);
            Assert.IsTrue(Quaternion.Dot(parent.transform.rotation, target.transform.rotation) > 0.999f);
        }

        [UnityTest]
        [TestCaseSource(nameof(snapTestCases))]
        public IEnumerator ObjectDoesNotSnapToParentIfNotSet(Vector3 parentPosition, Quaternion parentRotation)
        {
            // Given a set parent behavior,
            Vector3 originalPosition = new Vector3(456, 42, - 22);
            Quaternion originalRotation = Quaternion.Euler(34, -56, 190);
            ProcessSceneObject target = SpawnTestObject("Target", originalPosition, originalRotation, Vector3.one);
            ProcessSceneObject parent = SpawnTestObject("Parent", parentPosition, parentRotation, Vector3.one);
            IBehavior behavior = new SetParentBehavior(target, parent, false);

            // When the behavior completes,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            // Then the target object has not been parented, and its position stays the same.
            Assert.AreEqual(parent.transform, target.transform.parent);
            Assert.IsTrue((originalPosition - target.transform.position).sqrMagnitude < 0.001f);
            Assert.IsTrue(Quaternion.Dot(originalRotation, target.transform.rotation) > 0.999f);
        }

        [UnityTest]
        public IEnumerator ObjectSetToRootIfParentNotSet()
        {
            // Given a set parent behavior,
            ProcessSceneObject target = SpawnTestObject("Target", Vector3.zero, Quaternion.identity, Vector3.one);
            ProcessSceneObject parent = SpawnTestObject("Parent", Vector3.zero, Quaternion.identity, Vector3.one);
            target.transform.SetParent(parent.transform);
            IBehavior behavior = new SetParentBehavior(target, null);

            // When the behavior completes,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            // Then the target object has been unparented.
            Assert.AreEqual(null, target.transform.parent);
        }

        [UnityTest]
        [TestCaseSource(nameof(snapTestCases))]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt(Vector3 parentPosition, Quaternion parentRotation)
        {
            // Given a set parent behavior,
            Vector3 originalPosition = new Vector3(456, 42, -22);
            Quaternion originalRotation = Quaternion.Euler(34, -56, 190);
            ProcessSceneObject target = SpawnTestObject("Target", originalPosition, originalRotation, Vector3.one);
            ProcessSceneObject parent = SpawnTestObject("Parent", parentPosition, parentRotation, Vector3.one);
            IBehavior behavior = new SetParentBehavior(target, parent, true);

            // When we mark it to fast-forward and activate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            // Then it autocompletes immediately.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue((parentPosition - target.transform.position).sqrMagnitude < 0.001f);
            Assert.IsTrue(Quaternion.Dot(parentRotation, target.transform.rotation) > 0.999f);
            Assert.AreEqual(parent.transform, target.transform.parent);

            yield break;
        }

        [UnityTest]
        [TestCaseSource(nameof(snapTestCases))]
        public IEnumerator FastForwardInactiveBehaviorAndDeactivateIt(Vector3 parentPosition, Quaternion parentRotation)
        {
            // Given a set parent behavior,
            Vector3 originalPosition = new Vector3(456, 42, -22);
            Quaternion originalRotation = Quaternion.Euler(34, -56, 190);
            ProcessSceneObject target = SpawnTestObject("Target", originalPosition, originalRotation, Vector3.one);
            ProcessSceneObject parent = SpawnTestObject("Parent", parentPosition, parentRotation, Vector3.one);
            IBehavior behavior = new SetParentBehavior(target, parent, true);

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
            Assert.IsTrue((parentPosition - target.transform.position).sqrMagnitude < 0.001f);
            Assert.IsTrue(Quaternion.Dot(parentRotation, target.transform.rotation) > 0.999f);
            Assert.AreEqual(parent.transform, target.transform.parent);
        }

        [UnityTest]
        [TestCaseSource(nameof(snapTestCases))]
        public IEnumerator FastForwardActivatingBehavior(Vector3 parentPosition, Quaternion parentRotation)
        {
            // Given a set parent behavior,
            Vector3 originalPosition = new Vector3(456, 42, -22);
            Quaternion originalRotation = Quaternion.Euler(34, -56, 190);
            ProcessSceneObject target = SpawnTestObject("Target", originalPosition, originalRotation, Vector3.one);
            ProcessSceneObject parent = SpawnTestObject("Parent", parentPosition, parentRotation, Vector3.one);
            IBehavior behavior = new SetParentBehavior(target, parent, true);

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
            Assert.IsTrue((parentPosition - target.transform.position).sqrMagnitude < 0.001f);
            Assert.IsTrue(Quaternion.Dot(parentRotation, target.transform.rotation) > 0.999f);
            Assert.AreEqual(parent.transform, target.transform.parent);
        }

        [UnityTest]
        [TestCaseSource(nameof(snapTestCases))]
        public IEnumerator FastForwardDeactivatingBehavior(Vector3 parentPosition, Quaternion parentRotation)
        {
            // Given a set parent behavior,
            Vector3 originalPosition = new Vector3(456, 42, -22);
            Quaternion originalRotation = Quaternion.Euler(34, -56, 190);
            ProcessSceneObject target = SpawnTestObject("Target", originalPosition, originalRotation, Vector3.one);
            ProcessSceneObject parent = SpawnTestObject("Parent", parentPosition, parentRotation, Vector3.one);
            IBehavior behavior = new SetParentBehavior(target, parent, true);

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
            Assert.IsTrue((parentPosition - target.transform.position).sqrMagnitude < 0.001f);
            Assert.IsTrue(Quaternion.Dot(parentRotation, target.transform.rotation) > 0.999f);
            Assert.AreEqual(parent.transform, target.transform.parent);
        }
    }
}