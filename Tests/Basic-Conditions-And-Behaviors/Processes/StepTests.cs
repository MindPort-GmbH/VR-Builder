using System.Collections;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration;
using VRBuilder.Tests.Utils;
using VRBuilder.Tests.Utils.Mocks;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace VRBuilder.Core.Tests.Processes
{
    public class StepTests : RuntimeTests
    {
        [UnityTest]
        public IEnumerator ConditionCompletedAfterTimingBehaviorInStep()
        {
            float targetDuration = 0.5f;
            Step step = new Step("Step1");
            ICondition condition = new TimeoutCondition(targetDuration);
            Transition transition = new Transition();
            IBehavior behavior = new TimeoutBehaviorMock(targetDuration, targetDuration);
            transition.Data.Conditions.Add(condition);
            step.Data.Transitions.Data.Transitions.Add(transition);
            step.Data.Behaviors.Data.Behaviors.Add(behavior);
            step.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            step.LifeCycle.Activate();

            // Activation frame
            yield return null;
            step.Update();

            float startTime = Time.time;
            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                step.Update();
            }

            float behaviorDuration = Time.time - startTime;

            Assert.AreEqual(targetDuration, behaviorDuration,  Time.deltaTime);
            Assert.IsFalse(condition.IsCompleted);

            // Process frames
            yield return null;
            step.Update();
            yield return null;
            step.Update();
            yield return null;
            step.Update();

            startTime = Time.time;
            while (condition.IsCompleted == false)
            {
                yield return null;
                step.Update();
            }

            float conditionDuration = Time.time - startTime;

            Assert.AreEqual(targetDuration, conditionDuration, 2 * Time.deltaTime);
            Assert.IsTrue(condition.IsCompleted);
        }
    }
}
