using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using VRBuilder.Core;
using VRBuilder.Core.Conditions;

namespace VRBuilder.Tests.Utils
{
    public abstract class ConditionTests : RuntimeTests
    {
        protected abstract ICondition CreateDefaultCondition();

        [UnityTest]
        public IEnumerator NotCompleted()
        {
            // Create Condition
            ICondition condition = CreateDefaultCondition();
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is not completed
            Assert.IsFalse(condition.IsCompleted, "Condition should not be completed!");
        }

        [UnityTest]
        public IEnumerator FastForwardDoesNotCompleteCondition()
        {
            // Create Condition
            ICondition condition = CreateDefaultCondition();
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
