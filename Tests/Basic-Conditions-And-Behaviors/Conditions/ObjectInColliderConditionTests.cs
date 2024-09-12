using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Unity;

namespace VRBuilder.Core.Tests.Conditions
{
    [TestFixture]
    public class ObjectInColliderConditionTests : ObjectInTargetTestBase
    {
        [SetUp]
        public void SetUpColliderSceneObject()
        {
            // Setup collider process object
            BoxCollider boxCollider = TargetPositionObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            TargetPositionObject.AddComponent<ColliderWithTriggerProperty>();
            TargetProcessSceneObject = TargetPositionObject.GetOrAddComponent<ProcessSceneObject>();
        }

        [SetUp]
        public void SetUpTrackedSceneObject()
        {
            // Setup tracked process object
            TrackedObject.AddComponent<BoxCollider>();
            Rigidbody rigidbody = TrackedObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            TrackedProcessSceneObject = TrackedObject.AddComponent<ProcessSceneObject>();
        }

        [UnityTest]
        public IEnumerator CompleteWhenTargetObjectIsAtExactPositionAsCollider()
        {
            // Activate collider condition
            ObjectInColliderCondition condition = new ObjectInColliderCondition(TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>(), TrackedProcessSceneObject);
            condition.LifeCycle.Activate();

            yield return null;
            condition.Update();

            // Move tracked object to the target position
            TrackedObject.transform.position = TargetPositionObject.transform.position;
            yield return null;
            condition.Update();
            yield return null;
            condition.Update();

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted, "TargetInColliderCondition should be completed!");
        }

        [UnityTest]
        public IEnumerator CompleteWhenTargetObjectIsInsideCollider()
        {
            // Activate collider condition
            ObjectInColliderCondition condition = new ObjectInColliderCondition(TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>(), TrackedProcessSceneObject);
            condition.LifeCycle.Activate();

            yield return null;
            condition.Update();

            // Move tracked object to the target position
            TrackedObject.transform.position = TargetPositionObject.transform.position - PositionOffsetNearTarget;
            yield return null;
            condition.Update();
            yield return null;
            condition.Update();

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted, "TargetInColliderCondition should be completed!");
        }

        [UnityTest]
        public IEnumerator CompleteWhenTargetObjectIsAtExactPositionAsColliderOnStart()
        {
            // Move tracked object at the target position
            TrackedObject.transform.position = TargetPositionObject.transform.position;
            yield return null;

            // Activate collider condition
            ObjectInColliderCondition condition = new ObjectInColliderCondition(TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>(), TrackedProcessSceneObject);
            condition.LifeCycle.Activate();

            while (condition.IsCompleted == false)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted, "TargetInColliderCondition should be completed!");
        }

        [UnityTest]
        public IEnumerator CompleteWhenTargetObjectIsInsideColliderOnStart()
        {
            // Move tracked object at the target position
            TrackedObject.transform.position = TargetPositionObject.transform.position - PositionOffsetNearTarget;

            yield return null;

            // Activate collider condition
            ObjectInColliderCondition condition = new ObjectInColliderCondition(TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>(), TrackedProcessSceneObject);
            condition.LifeCycle.Activate();

            while (condition.IsCompleted == false)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted, "TargetInColliderCondition should be completed!");
        }

        [UnityTest]
        public IEnumerator CompleteWhenTargetObjectIsInsideColliderWithDuration()
        {
            // Set the target duration
            const float targetDuration = 0.1f;

            // Activate collider condition
            ObjectInColliderCondition condition = new ObjectInColliderCondition(TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>(), TrackedProcessSceneObject, targetDuration);
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Move tracked object to the target position
            TrackedObject.transform.position = TargetPositionObject.transform.position;

            Assert.IsFalse(condition.IsCompleted);

            yield return null;
            condition.Update();

            float startTime = Time.time;
            while (condition.IsCompleted == false)
            {
                yield return null;
                condition.Update();
            }

            float duration = Time.time - startTime;

            // Assert that condition is completed after the specified time.
            Assert.AreEqual(targetDuration, duration, Time.deltaTime);
            Assert.IsTrue(condition.IsCompleted, "TargetInColliderCondition should be completed!");
        }

        [UnityTest]
        public IEnumerator DontCompleteWhenTargetObjectLeavesColliderEarly()
        {
            // Set the target duration
            const float targetDuration = 0.1f;

            // Activate collider condition
            ObjectInColliderCondition condition = new ObjectInColliderCondition(TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>(), TrackedProcessSceneObject, targetDuration);
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Move tracked object to the target position
            TrackedObject.transform.position = TargetPositionObject.transform.position;

            float startTime = Time.time;
            while (Time.time < startTime + targetDuration * 0.3f)
            {
                yield return null;
                condition.Update();
            }

            // Move tracked object away from target position before condition is completed
            TrackedObject.transform.position = PositionFarFromTarget;

            startTime = Time.time;
            while (Time.time < startTime + targetDuration * 0.8f)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is not completed
            Assert.IsFalse(condition.IsCompleted, "TargetInColliderCondition should not be completed!");
        }

        [UnityTest]
        public IEnumerator NotCompleted()
        {
            // Activate collider condition
            ObjectInColliderCondition condition = new ObjectInColliderCondition(TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>(), TrackedProcessSceneObject);
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is not completed
            Assert.IsFalse(condition.IsCompleted, "TargetInColliderCondition should not be completed!");
        }

        [UnityTest]
        public IEnumerator DontCompleteWhenWrongObjectEntersCollider()
        {
            // In addition to setup phase, also setup an additional object
            GameObject wrongObj = new GameObject("Wrong Object");
            wrongObj.transform.position = PositionFarFromTarget;
            wrongObj.AddComponent<BoxCollider>();
            wrongObj.AddComponent<Rigidbody>();
            ProcessSceneObject wrongProcessSceneObject = wrongObj.AddComponent<ProcessSceneObject>();

            // Activate collider condition
            ObjectInColliderCondition condition = new ObjectInColliderCondition(TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>(), TrackedProcessSceneObject);
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Move tracked object to the target position
            wrongProcessSceneObject.transform.position = TargetPositionObject.transform.position;

            float startTime = Time.time;
            while (Time.time < startTime + 0.1f)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is not completed
            Assert.IsFalse(condition.IsCompleted, "TargetInColliderCondition should not be completed!");
        }

        [UnityTest]
        public IEnumerator AutoCompleteActive()
        {
            // Given an object in an activated collider condition,
            ObjectInColliderCondition condition = new ObjectInColliderCondition(TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>(), TrackedProcessSceneObject);

            bool isColliding = false;
            TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>().EnteredTrigger += (sender, args) => isColliding = true;

            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // When you autocomplete it,
            condition.Autocomplete();

            // Then condition is activated and the object is moved into collider.
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);
            Assert.IsTrue(isColliding);
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator AutoCompleteActiveWithHigherRequiredObjects()
        {
            // Given an activated object in collider condition,
            ObjectInColliderCondition condition = new ObjectInColliderCondition(TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>(), TrackedProcessSceneObject, 42);

            bool isColliding = false;
            TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>().EnteredTrigger += (_, _) => isColliding = true;

            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // When you fast-forward it
            condition.LifeCycle.MarkToFastForward();

            // Then nothing happens.
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);
            Assert.IsFalse(condition.IsCompleted);
            Assert.IsFalse(isColliding);
        }

        [UnityTest]
        public IEnumerator AutoCompleteHigherRequiredCountInCollider()
        {
            // Given an activated object in collider condition,
            ObjectInColliderCondition condition = new ObjectInColliderCondition(TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>(), TrackedProcessSceneObject, 0, 42);

            bool isColliding = false;
            TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>().EnteredTrigger += (_, _) => isColliding = true;

            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // When you autocomplete it,
            condition.Autocomplete();

            // Then condition is activated and the object is moved into collider.
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);
            Assert.IsTrue(isColliding);
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator UncompletableObjectCountInCollider()
        {
            // Move tracked object at the target position
            TrackedObject.transform.position = TargetPositionObject.transform.position - PositionOffsetNearTarget;

            yield return null;

            // Activate collider condition
            ObjectInColliderCondition condition = new ObjectInColliderCondition(TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>(), TrackedProcessSceneObject, 0, 42);
            condition.LifeCycle.Activate();

            float startTime = Time.time;
            while (startTime + 5f > Time.time)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is now uncompleted
            Assert.IsFalse(condition.IsCompleted, "TargetInColliderCondition should be not completed!");
        }

        [UnityTest]
        public IEnumerator CompletableZeroObjectCountInCollider()
        {
            // Activate collider condition
            ObjectInColliderCondition condition = new ObjectInColliderCondition(TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>(), TrackedProcessSceneObject, 0, 0);
            condition.LifeCycle.Activate();
            
            yield return null;
            condition.Update();
            
            yield return null;
            condition.Update();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Then condition is activated and the object is moved into collider.
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);
            Assert.IsTrue(condition.IsCompleted, "TargetInColliderCondition should always be completed!");
        }

        [UnityTest]
        public IEnumerator CompletableObjectCountInCollider()
        {
            GameObject trackedObject2 = new GameObject("Tracked Object 2");
            trackedObject2.transform.position = PositionFarFromTarget;
            trackedObject2.AddComponent<ProcessSceneObject>();
            GameObject trackedObject3 = new GameObject("Tracked Object 3");
            trackedObject3.transform.position = PositionFarFromTarget;
            trackedObject3.AddComponent<ProcessSceneObject>();
            
            MultipleSceneObjectReference multipleObjects = new MultipleSceneObjectReference( new []
            {
                TrackedProcessSceneObject.GetComponent<ProcessSceneObject>().Guid,
                trackedObject2.GetComponent<ProcessSceneObject>().Guid,
                trackedObject3.GetComponent<ProcessSceneObject>().Guid
            }); 
            
            // Move tracked object at the target position
            TrackedObject.transform.position = TargetPositionObject.transform.position - PositionOffsetNearTarget;
            trackedObject2.transform.position = TargetPositionObject.transform.position - PositionOffsetNearTarget;
            trackedObject3.transform.position = TargetPositionObject.transform.position - PositionOffsetNearTarget;

            // Activate collider condition
            ObjectInColliderCondition condition = new ObjectInColliderCondition(TargetProcessSceneObject.GetProperty<ColliderWithTriggerProperty>(), multipleObjects.Guids, 0, 3);
            condition.LifeCycle.Activate();
            
            yield return null;

            // Move tracked object to the target position
            TrackedObject.transform.position = TargetPositionObject.transform.position;
            
            Assert.IsFalse(condition.IsCompleted);
            yield return null;
            condition.Update();
            
            trackedObject2.transform.position = TargetPositionObject.transform.position;
            
            Assert.IsFalse(condition.IsCompleted);
            yield return null;
            condition.Update();
            
            trackedObject3.transform.position = TargetPositionObject.transform.position;
            
            yield return null;
            condition.Update();
            yield return null;
            condition.Update();

            // Assert that condition is now uncompleted
            Assert.IsTrue(condition.IsCompleted, "TargetInColliderCondition should be completed!");
        }
    }
}
