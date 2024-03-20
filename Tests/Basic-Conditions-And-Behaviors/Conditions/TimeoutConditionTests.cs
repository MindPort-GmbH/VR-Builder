using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Tests.RuntimeUtils;

namespace VRBuilder.Core.Tests.Conditions
{
    public class TimeoutConditionTests : RuntimeTests
    {
        [UnityTest]
        public IEnumerator ConditionIsActivated()
        {
            // Setup Condition
            TimeoutCondition condition = new TimeoutCondition(0.2f);

            // Activate condition and wait
            condition.LifeCycle.Activate();

            float startTime = Time.time;
            while (Time.time < startTime + 0.1f)
            {
                yield return null;
                condition.Update();
            }

            // Check state is correct
            Assert.AreEqual(condition.LifeCycle.Stage, Stage.Active, "TimeoutCondition should be active");
        }

        [UnityTest]
        public IEnumerator ActivationEventsAreEmitted()
        {
            // Setup Condition
            TimeoutCondition condition = new TimeoutCondition(0.1f);
            bool isActivationStarted = false;
            bool isActivated = false;
            condition.LifeCycle.StageChanged += (sender, args) =>
            {
                if (args.Stage == Stage.Activating)
                {
                    isActivationStarted = true;
                }


                if (args.Stage == Stage.Active)
                {
                    isActivated = true;
                }
            };

            // Activate condition and wait
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Check events got called
            Assert.IsTrue(isActivationStarted, "TimeoutCondition was not activated");
            Assert.IsTrue(isActivated, "TimeoutCondition did not call activated");
        }

        [UnityTest]
        public IEnumerator ConditionIsCompleted()
        {
            // Given a TimeoutCondition which is completed after 200ms.
            float targetDuration = 0.2f;
            TimeoutCondition condition = new TimeoutCondition(targetDuration);

            // When the condition is activated.
            condition.LifeCycle.Activate();

            // Activation frame
            yield return null;
            condition.Update();

            float startTime = Time.time;
            while (condition.IsCompleted == false)
            {
                yield return null;
                condition.Update();
            }

            float duration = Time.time - startTime;

            // Then the condition is completed after the specified time.
            Assert.AreEqual(targetDuration, duration, Time.deltaTime);
            Assert.IsTrue(condition.IsCompleted, "TimeoutCondition is not completed but should be!");
        }

        [UnityTest]
        public IEnumerator ConditionIsNotCompleted()
        {
            // Create TimeoutCondition which is completed after 100ms
            TimeoutCondition condition = new TimeoutCondition(0.1f);
            // Start counter
            condition.LifeCycle.Activate();

            yield return null;
            condition.Update();

            // Condition should be active now
            Assert.AreEqual(condition.LifeCycle.Stage, Stage.Active, "TimeoutCondition is not active");
            // Check if condition is still not completed
            Assert.IsFalse(condition.IsCompleted, "TimeoutCondition is already completed!");
        }

        [UnityTest]
        public IEnumerator AutoCompleteActive()
        {
            // Given a timeout condition
            TimeoutCondition condition = new TimeoutCondition(0.1f);

            // When you activate and autocomplete it,
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            condition.Autocomplete();

            // Then it is completed.
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator FastForwardDoesNotCompleteCondition()
        {
            // Given a timeout condition
            TimeoutCondition condition = new TimeoutCondition(0.1f);

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
        }
    }
}
