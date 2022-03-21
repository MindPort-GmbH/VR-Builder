using System.Collections;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Tests.Utils;
using UnityEngine.TestTools;
using UnityEngine;
using NUnit.Framework;
using Object = UnityEngine.Object;

namespace VRBuilder.Core.Tests.Behaviors
{
#pragma warning disable 618
    public class UnlockBehaviorTests : RuntimeTests
    {
        private const string targetName = "TestReference";

        [UnityTest]
        public IEnumerator CreateUnlockBehavior()
        {
            // Given an game object with a changed unique name,
            GameObject gameObject = new GameObject("Test");
            ProcessSceneObject targetObject = gameObject.AddComponent<ProcessSceneObject>();
            targetObject.ChangeUniqueName(targetName);

            // When we reference it by reference or unique name in the UnlockObjectBehavior,

            UnlockObjectBehavior unlock1 = new UnlockObjectBehavior(targetObject);
            UnlockObjectBehavior unlock2 = new UnlockObjectBehavior(targetName);

            // Then it is the same object.
            Assert.AreEqual(targetObject, unlock1.Data.Target.Value);
            Assert.AreEqual(targetObject, unlock2.Data.Target.Value);

            // Cleanup created game objects.
            Object.DestroyImmediate(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator UnlockBehaviorOnUnlockedObject()
        {
            // Given an UnlockObjectBehavior, an unlocked game object, and a full process step,
            GameObject gameObject = new GameObject("Test");
            ProcessSceneObject targetObject = gameObject.AddComponent<ProcessSceneObject>();

            Step step = new Step("TestStep");
            Transition transition = new Transition();
            step.Data.Transitions.Data.Transitions.Add(transition);

            UnlockObjectBehavior unlockBehavior = new UnlockObjectBehavior(targetObject);
            step.Data.Behaviors.Data.Behaviors.Add(unlockBehavior);
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

            // Then the game object was unlocked initially, unlocked during step execution, and unlocked after the completion.
            Assert.IsFalse(isLockedInitially, "Object should not be locked initially");
            Assert.IsFalse(isLockedDuringStep, "Object should unlocked during step");
            Assert.IsFalse(isLockedInEnd, "Object should be unlocked in the end");

            // Cleanup created game objects.
            Object.DestroyImmediate(gameObject);
            yield return null;
        }


        [UnityTest]
        public IEnumerator UnlockBehaviorOnLockedObject()
        {
            // Given an UnlockObjectBehavior, a locked game object, and a full process step,
            GameObject gameObject = new GameObject("Test");
            ProcessSceneObject targetObject = gameObject.AddComponent<ProcessSceneObject>();

            Step step = new Step("TestStep");
            Transition transition = new Transition();
            step.Data.Transitions.Data.Transitions.Add(transition);

            UnlockObjectBehavior unlockBehavior = new UnlockObjectBehavior(targetObject);
            step.Data.Behaviors.Data.Behaviors.Add(unlockBehavior);
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

            // Then the game object was locked initially, unlocked during step execution, and locked after the completion.
            Assert.IsTrue(isLockedInitially, "Object should be locked initially");
            Assert.IsFalse(isLockedDuringStep, "Object should be unlocked during step");
            Assert.IsTrue(isLockedInEnd, "Object should be locked in the end");

            // Cleanup created game objects.
            Object.DestroyImmediate(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehavior()
        {
            // Given an UnlockObjectBehavior and an unlocked game object,
            GameObject gameObject = new GameObject("Test");
            ProcessSceneObject targetObject = gameObject.AddComponent<ProcessSceneObject>();

            UnlockObjectBehavior behavior = new UnlockObjectBehavior(targetObject);

            // When we mark the behavior to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it is still in PendingActivation state because it hasn't been activated yet.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);

            // Cleanup created game objects.
            Object.DestroyImmediate(gameObject);

            yield return null;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given an UnlockObjectBehavior and a locked game object,
            GameObject gameObject = new GameObject("Test");
            ProcessSceneObject targetObject = gameObject.AddComponent<ProcessSceneObject>();

            UnlockObjectBehavior behavior = new UnlockObjectBehavior(targetObject);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            targetObject.SetLocked(true);

            bool isLockedInitially = targetObject.IsLocked;

            // When we mark the behavior to fast-forward and activate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            // Then it should work without any differences because the behavior is done immediately anyways.
            bool isLockedDuringStep = targetObject.IsLocked;

            behavior.LifeCycle.Deactivate();

            bool isLockedInEnd = targetObject.IsLocked;

            Assert.IsTrue(isLockedInitially, "Object should be locked initially");
            Assert.IsFalse(isLockedDuringStep, "Object should be unlocked during step");
            Assert.IsTrue(isLockedInEnd, "Object should be locked in the end");

            // Cleanup created game objects.
            Object.DestroyImmediate(gameObject);

            yield return null;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBehavior()
        {
            // Given an UnlockObjectBehavior and a locked game object,
            GameObject gameObject = new GameObject("Test");
            ProcessSceneObject targetObject = gameObject.AddComponent<ProcessSceneObject>();

            UnlockObjectBehavior behavior = new UnlockObjectBehavior(targetObject);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            targetObject.SetLocked(true);

            bool isLockedInitially = targetObject.IsLocked;

            behavior.LifeCycle.Activate();

            // When we mark the behavior to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it should work without any differences because the behavior is done immediately anyways.
            bool isLockedDuringStep = targetObject.IsLocked;

            behavior.LifeCycle.Deactivate();

            bool isLockedInEnd = targetObject.IsLocked;

            Assert.IsTrue(isLockedInitially, "Object should be locked initially");
            Assert.IsFalse(isLockedDuringStep, "Object should be unlocked during step");
            Assert.IsTrue(isLockedInEnd, "Object should be locked in the end");

            // Cleanup created game objects.
            Object.DestroyImmediate(gameObject);

            yield return null;
        }
    }
}
