using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Tests.Utils;

namespace VRBuilder.Core.Tests.Behaviors
{
#pragma warning disable 618
    public class LockBehaviorTests : RuntimeTests
    {
        private const string targetName = "TestReference";

        [UnityTest]
        public IEnumerator CreateLockBehavior()
        {
            // Given an game object with a changed unique name,
            GameObject gameObject = new GameObject("Test");
            ProcessSceneObject targetObject = gameObject.AddComponent<ProcessSceneObject>();

            // When we reference it by reference or unique name in the LockObjectBehavior,
            LockObjectBehavior lock1 = new LockObjectBehavior(targetObject);
            LockObjectBehavior lock2 = new LockObjectBehavior(targetName);

            // Then it is the same object.
            Assert.AreEqual(targetObject, lock1.Data.Target.Value);
            Assert.AreEqual(targetObject, lock2.Data.Target.Value);

            yield break;
        }

        [UnityTest]
        public IEnumerator LockBehaviorOnUnlockedObject()
        {
            // Given a LockObjectBehavior, an unlocked game object, and a full process step,
            GameObject gameObject = new GameObject("Test");
            ProcessSceneObject targetObject = gameObject.AddComponent<ProcessSceneObject>();

            Step step = new Step("TestStep");
            Transition transition = new Transition();
            step.Data.Transitions.Data.Transitions.Add(transition);

            LockObjectBehavior lockBehavior = new LockObjectBehavior(targetObject);
            step.Data.Behaviors.Data.Behaviors.Add(lockBehavior);

            step.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we fulfill the step,
            bool isLockedInitially = targetObject.IsLocked;

            step.LifeCycle.Activate();

            while (step.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                step.Update();
            }

            bool isLockedDuringStep = targetObject.IsLocked;

            step.LifeCycle.Deactivate();

            while (step.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                step.Update();
            }

            bool isLockedInEnd = targetObject.IsLocked;

            // Then the game object was unlocked initially, locked during step execution, and unlocked after the completion.
            Assert.IsFalse(isLockedInitially, "Object should not be locked initially");
            Assert.IsTrue(isLockedDuringStep, "Object should be locked during step");
            Assert.IsFalse(isLockedInEnd, "Object should not be locked in the end");

            yield break;
        }


        [UnityTest]
        public IEnumerator LockBehaviorOnLockedObject()
        {
            // Given a LockObjectBehavior, an locked game object, and a full process step,
            GameObject gameObject = new GameObject("Test");
            ProcessSceneObject targetObject = gameObject.AddComponent<ProcessSceneObject>();

            Step step = new Step("TestStep");
            Transition transition = new Transition();
            step.Data.Transitions.Data.Transitions.Add(transition);

            LockObjectBehavior lockBehavior = new LockObjectBehavior(targetObject);
            step.Data.Behaviors.Data.Behaviors.Add(lockBehavior);

            step.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            targetObject.SetLocked(true);

            // When we fulfill the step,
            bool isLockedInitially = targetObject.IsLocked;

            step.LifeCycle.Activate();

            while (step.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                step.Update();
            }

            bool isLockedDuringStep = targetObject.IsLocked;

            step.LifeCycle.Deactivate();

            while (step.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                step.Update();
            }

            bool isLockedInEnd = targetObject.IsLocked;

            // Then the game object was locked initially, locked during step execution, and locked after the completion.
            Assert.IsTrue(isLockedInitially, "Object should be locked initially");
            Assert.IsTrue(isLockedDuringStep, "Object should be locked during step");
            Assert.IsTrue(isLockedInEnd, "Object should be locked in the end");

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehavior()
        {
            // Given a LockObjectBehavior and an unlocked game object,
            GameObject gameObject = new GameObject("Test");
            ProcessSceneObject targetObject = gameObject.AddComponent<ProcessSceneObject>();

            LockObjectBehavior behavior = new LockObjectBehavior(targetObject);

            // When we mark the behavior to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it is still in PendingActivation state because it hasn't been activated yet.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);

            // Cleanup created game objects.
            Object.DestroyImmediate(gameObject);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given a LockObjectBehavior, an unlocked game object, and a full process step,
            GameObject gameObject = new GameObject("Test");
            ProcessSceneObject targetObject = gameObject.AddComponent<ProcessSceneObject>();

            Step step = new Step("TestStep");
            Transition transition = new Transition();
            step.Data.Transitions.Data.Transitions.Add(transition);

            LockObjectBehavior behavior = new LockObjectBehavior(targetObject);
            step.Data.Behaviors.Data.Behaviors.Add(behavior);

            step.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            bool isLockedInitially = targetObject.IsLocked;

            // When we mark the behavior to fast-forward and activate the step,
            behavior.LifeCycle.MarkToFastForward();
            step.LifeCycle.Activate();

            yield return null;
            step.Update();

            // Then it should work without any differences because the behavior is done immediately anyways.
            bool isLockedDuringStep = targetObject.IsLocked;

            step.LifeCycle.Deactivate();

            while (step.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                step.Update();
            }

            bool isLockedInEnd = targetObject.IsLocked;

            Assert.IsFalse(isLockedInitially, "Object should not be locked initially");
            Assert.IsTrue(isLockedDuringStep, "Object should be locked during step");
            Assert.IsFalse(isLockedInEnd, "Object should not be locked in the end");

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBehavior()
        {
            // Given a LockObjectBehavior, an unlocked game object, and an activated full process step,
            GameObject gameObject = new GameObject("Test");
            ProcessSceneObject targetObject = gameObject.AddComponent<ProcessSceneObject>();

            Step step = new Step("TestStep");
            Transition transition = new Transition();
            step.Data.Transitions.Data.Transitions.Add(transition);

            LockObjectBehavior behavior = new LockObjectBehavior(targetObject);
            step.Data.Behaviors.Data.Behaviors.Add(behavior);

            step.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            bool isLockedInitially = targetObject.IsLocked;

            step.LifeCycle.Activate();

            yield return null;
            step.Update();

            // When we mark the behavior to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it should work without any differences because the behavior is done immediately anyways.
            bool isLockedDuringStep = targetObject.IsLocked;

            step.LifeCycle.Deactivate();

            while (step.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                step.Update();
            }

            bool isLockedInEnd = targetObject.IsLocked;

            Assert.IsFalse(isLockedInitially, "Object should not be locked initially");
            Assert.IsTrue(isLockedDuringStep, "Object should be locked during step");
            Assert.IsFalse(isLockedInEnd, "Object should not be locked in the end");

            yield break;
        }
    }
}
