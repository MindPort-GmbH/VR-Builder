using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Tests.Utils;

namespace VRBuilder.Core.Tests.Behaviors
{
    public class ScalingBehaviorTests : RuntimeTests
    {
        private const string targetName = "TestReference";
        private readonly Vector3 newScale = new Vector3(15, 10, 7.5f);
        private readonly IMode defaultMode = new Mode("Default", new WhitelistTypeRule<IOptional>());

        [UnityTest]
        public IEnumerator DoneAfterTime()
        {
            // Given a complete scaling behavior with a positive duration,
            const float duration = 0.05f;

            GameObject target = new GameObject(targetName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            Vector3 endScale = target.transform.localScale + newScale;

            IBehavior behavior = new ScalingBehavior(new SceneObjectReference(targetName), endScale, duration);
            behavior.Configure(defaultMode);

            // When we activate the behavior and wait for it's delay time,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            yield return null;
            behavior.Update();

            float startTime = Time.time;
            while (Time.time < startTime + duration)
            {
                Assert.AreEqual(Stage.Activating, behavior.LifeCycle.Stage);
                Assert.IsFalse(target.transform.localScale == endScale);
                yield return null;
                behavior.Update();
            }

            // Then the behavior should be active and the object is scaled correctly.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue(target.transform.localScale == endScale);
        }

        [UnityTest]
        public IEnumerator RunsInstantlyWhenDelayTimeIsZero()
        {
            // Given a complete scaling behavior with duration time == 0,
            const float duration = 0f;

            GameObject target = new GameObject(targetName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            Vector3 endScale = target.transform.localScale + newScale;

            IBehavior behavior = new ScalingBehavior(new SceneObjectReference(targetName), endScale, duration);
            behavior.Configure(defaultMode);

            // When we activate it and wait one update cycle,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            yield return null;
            behavior.Update();

            // Then the behavior is activated immediately and the object is scaled correctly.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue(target.transform.localScale == endScale);
        }

        [UnityTest]
        public IEnumerator NegativeTimeCompletesImmediately()
        {
            // Given a complete scaling behavior with negative duration time,
            const float duration = -0.05f;

            GameObject target = new GameObject(targetName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            Vector3 endScale = target.transform.localScale + newScale;

            IBehavior behavior = new ScalingBehavior(new SceneObjectReference(targetName), endScale, duration);
            behavior.Configure(defaultMode);

            // When we activate it and wait one update cycle,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            yield return null;
            behavior.Update();

            // Then the behavior is activated immediately and the object is scaled correctly.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue(target.transform.localScale == endScale);

            yield break;
        }

        [UnityTest]
        public IEnumerator ZeroScaleCompletes()
        {
            // Given a complete scaling behavior with duration time == 0 and scale == (0, 0, 0),
            const float duration = 0f;

            GameObject target = new GameObject(targetName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            Vector3 endScale = Vector3.zero;

            IBehavior behavior = new ScalingBehavior(new SceneObjectReference(targetName), endScale, duration);
            behavior.Configure(defaultMode);

            // When we activate it and wait one update cycle,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            yield return null;
            behavior.Update();

            // Then the behavior is activated immediately and the object is scaled correctly.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue(target.transform.localScale == endScale);
        }

        [UnityTest]
        public IEnumerator NegativeScaleCompletes()
        {
            // Given a complete scaling behavior with duration time == 0 and scale == (-1, -1, -1),
            const float duration = 0f;

            GameObject target = new GameObject(targetName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            Vector3 endScale = new Vector3(-1, -1, -1);

            IBehavior behavior = new ScalingBehavior(new SceneObjectReference(targetName), endScale, duration);
            behavior.Configure(defaultMode);

            // When we activate it and wait one update cycle,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            yield return null;
            behavior.Update();

            // Then the behavior is activated immediately and the object is scaled correctly.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue(target.transform.localScale == endScale);
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehavior()
        {
            // Given a complete scaling behavior with a positive duration,
            const float duration = 0.05f;

            GameObject target = new GameObject(targetName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            Vector3 endScale = target.transform.localScale + newScale;

            IBehavior behavior = new ScalingBehavior(new SceneObjectReference(targetName), endScale, duration);
            behavior.Configure(defaultMode);

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it doesn't autocomplete because it hasn't been activated yet.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
            Assert.IsFalse(target.transform.localScale == endScale);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given a complete scaling behavior with a positive duration,
            const float duration = 0.05f;

            GameObject target = new GameObject(targetName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            Vector3 endScale = target.transform.localScale + newScale;

            IBehavior behavior = new ScalingBehavior(new SceneObjectReference(targetName), endScale, duration);
            behavior.Configure(defaultMode);

            // When we mark it to fast-forward and activate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            // Then the behavior is activated immediately and the object is scaled correctly.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue(target.transform.localScale == endScale);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBehavior()
        {
            // Given an active and complete scaling behavior with a positive duration,
            const float duration = 0.05f;

            GameObject target = new GameObject(targetName);
            ProcessSceneObject positionProvider = target.AddComponent<ProcessSceneObject>();

            Vector3 endScale = target.transform.localScale + newScale;

            IBehavior behavior = new ScalingBehavior(new SceneObjectReference(targetName), endScale, duration);
            behavior.Configure(defaultMode);

            behavior.LifeCycle.Activate();

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then the behavior is activated immediately and the object is scaled correctly.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue(target.transform.localScale == endScale);

            yield break;
        }
    }
}
