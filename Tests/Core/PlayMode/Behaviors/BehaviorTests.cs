using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Tests.RuntimeUtils;

namespace VRBuilder.Core.Tests.Utils
{
    public abstract class BehaviorTests : RuntimeTests
    {
        protected abstract IBehavior CreateDefaultBehavior();

        [UnityTest]
        public IEnumerator FastForwardInactiveBehavior()
        {
            // Given a behavior,
            IBehavior behavior = CreateDefaultBehavior();

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it doesn't autocomplete because it hasn't been activated yet.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);

            yield break;
        }
    }
}
