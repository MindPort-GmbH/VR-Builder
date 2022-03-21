using System.Collections;
using VRBuilder.Core.Behaviors;
using VRBuilder.Tests.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace VRBuilder.Core.Tests.Behaviors
{
    public class DelayBehaviorTests : RuntimeTests
    {
        [UnityTest]
        public IEnumerator DoneAfterTime()
        {
            // Given a delayed activation behavior with a positive delay time,
            float delay = 0.1f;
            IBehavior behavior = new DelayBehavior(delay);

            // When we activate it and wait for it to be active,
            behavior.LifeCycle.Activate();

            float startTime = Time.time;
            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                behavior.Update();
                yield return null;
            }
            float duration = Time.time - startTime;

            // Then the behavior should be active after the specified delay, within a margin of error.
            Assert.GreaterOrEqual(duration, delay);
            Assert.LessOrEqual(duration, delay + 0.05f);
        }

        [UnityTest]
        public IEnumerator RunsInstantlyWhenDelayTimeIsZero()
        {
            // Given a delayed activation behavior with delay time == 0,
            IBehavior parentBehavior = new DelayBehavior(0f);

            // When we activate it,
            parentBehavior.LifeCycle.Activate();
            parentBehavior.Update();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, parentBehavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator NegativeTimeCompletesImmediately()
        {
            // Given a delayed activation behavior with negative delay time,
            DelayBehavior behavior = new DelayBehavior(-0.25f);

            // When we activate it,
            behavior.LifeCycle.Activate();
            behavior.Update();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehavior()
        {
            // Given a delayed activation behavior with a positive delay time,
            IBehavior behavior = new DelayBehavior(0.05f);

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it doesn't autocomplete because it hasn't been activated yet.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given a delayed activation behavior with a positive delay time,
            IBehavior behavior = new DelayBehavior(0.1f);

            // When we mark it to fast-forward and activate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBehavior()
        {
            // Given an active delayed activation behavior with a positive delay time,
            IBehavior behavior = new DelayBehavior(0.05f);

            behavior.LifeCycle.Activate();

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);

            yield break;
        }
    }
}
