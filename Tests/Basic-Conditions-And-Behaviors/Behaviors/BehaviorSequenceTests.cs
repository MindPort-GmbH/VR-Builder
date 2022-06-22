using System.Collections;
using System.Collections.Generic;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Tests.Utils;
using VRBuilder.Tests.Utils.Mocks;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace VRBuilder.Core.Tests.Behaviors
{
    public class BehaviorSequenceTests : RuntimeTests
    {
        private const float shortDelay = 0.05f;

        [UnityTest]
        public IEnumerator DoNotRepeat()
        {
            // Given a behaviors sequence with RepeatsMode = Once,
            DelayBehavior childBehavior = new DelayBehavior(shortDelay);
            BehaviorSequence sequence = new BehaviorSequence(false, new List<IBehavior> { childBehavior });

            // When we activate it,
            sequence.LifeCycle.Activate();

            yield return null;
            sequence.Update();

            // Then it completes its activation only after every child behavior was activated once.
            Assert.AreEqual(Stage.Activating, sequence.LifeCycle.Stage);
            Assert.AreEqual(Stage.Activating, childBehavior.LifeCycle.Stage);

            while (childBehavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                sequence.Update();
            }

            while (sequence.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                sequence.Update();
                Assert.AreNotEqual(Stage.Activating, childBehavior.LifeCycle.Stage);
            }

            Assert.AreEqual(Stage.Active, sequence.LifeCycle.Stage);

            // Cleanup.
            sequence.LifeCycle.Deactivate();
        }

        [UnityTest]
        public IEnumerator BehaviorsActivatedInSuccession()
        {
            // Given a sequence with two behaviors,
            BehaviorSequence sequence = new BehaviorSequence(true, new List<IBehavior>
            {
                new DelayBehavior(shortDelay),
                new DelayBehavior(shortDelay)
            });

            // When we activate it,
            sequence.LifeCycle.Activate();

            // One is activated only after the previous one is deactivated.
            yield return null;
            sequence.Update();

            IList<IBehavior> behaviors = sequence.Data.Behaviors;
            Assert.AreEqual(Stage.Activating, behaviors[0].LifeCycle.Stage);

            while (behaviors[0].LifeCycle.Stage != Stage.Inactive)
            {
                Assert.AreEqual(Stage.Inactive, behaviors[1].LifeCycle.Stage);
                yield return null;
                sequence.Update();
            }

            yield return null;
            sequence.Update();

            Assert.AreEqual(Stage.Activating, behaviors[1].LifeCycle.Stage);

            // Cleanup.
            sequence.LifeCycle.Deactivate();
        }

        [UnityTest]
        public IEnumerator ActivateOnlyAfterOnePass()
        {
            // Given a behaviors sequence,
            EndlessBehaviorMock endlessBehaviorMock = new EndlessBehaviorMock();
            BehaviorSequence sequence = new BehaviorSequence(true, new List<IBehavior> { endlessBehaviorMock });

            // When we activate it,
            sequence.LifeCycle.Activate();

            yield return null;
            sequence.Update();

            // Then it is activated only after one pass.
            endlessBehaviorMock.LifeCycle.MarkToFastForward();

            yield return null;
            sequence.Update();

            yield return null;
            sequence.Update();

            Assert.AreEqual(Stage.Active, sequence.LifeCycle.Stage);

            // Cleanup.
            sequence.LifeCycle.Deactivate();
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehavior()
        {
            // Given a sequence with two behaviors,
            BehaviorSequence sequence = new BehaviorSequence(false, new List<IBehavior>
            {
                new DelayBehavior(shortDelay),
                new DelayBehavior(shortDelay)
            });

            // When we mark it to fast-forward,
            sequence.LifeCycle.MarkToFastForward();

            // Then it doesn't autocomplete because it hasn't been activated yet.
            Assert.AreEqual(Stage.Inactive, sequence.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given a sequence with two behaviors,
            BehaviorSequence sequence = new BehaviorSequence(false, new List<IBehavior>
            {
                new DelayBehavior(shortDelay),
                new DelayBehavior(shortDelay)
            });

            // When we mark it to fast-forward and activate it,
            sequence.LifeCycle.MarkToFastForward();
            sequence.LifeCycle.Activate();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, sequence.LifeCycle.Stage);

            // Cleanup.
            sequence.LifeCycle.Deactivate();

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveRepeatingBehaviorAndActivateIt()
        {
            // Given a sequence with two behaviors,
            BehaviorSequence sequence = new BehaviorSequence(true, new List<IBehavior>
            {
                new DelayBehavior(shortDelay),
                new DelayBehavior(shortDelay)
            });

            // When we mark it to fast-forward and activate it,
            sequence.LifeCycle.MarkToFastForward();
            sequence.LifeCycle.Activate();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, sequence.LifeCycle.Stage);

            // Cleanup.
            sequence.LifeCycle.Deactivate();

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBehavior()
        {
            // Given a sequence with two behaviors,
            BehaviorSequence sequence = new BehaviorSequence(false, new List<IBehavior>
            {
                new DelayBehavior(shortDelay),
                new DelayBehavior(shortDelay)
            });

            sequence.LifeCycle.Activate();

            // When we mark it to fast-forward,
            sequence.LifeCycle.MarkToFastForward();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, sequence.LifeCycle.Stage);

            // Cleanup.
            sequence.LifeCycle.Deactivate();

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingRepeatingBehavior()
        {
            // Given a sequence with two behaviors,
            BehaviorSequence sequence = new BehaviorSequence(true, new List<IBehavior>
            {
                new DelayBehavior(shortDelay),
                new DelayBehavior(shortDelay)
            });

            sequence.LifeCycle.Activate();

            // When we mark it to fast-forward,
            sequence.LifeCycle.MarkToFastForward();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, sequence.LifeCycle.Stage);

            // Cleanup.
            sequence.LifeCycle.Deactivate();

            yield break;
        }

        [UnityTest]
        public IEnumerator SkipChildNotWhenItIsExecuted()
        {
            // Given an activating behavior sequence of one not optional and one optional behavior,
            OptionalEndlessBehaviorMock optional = new OptionalEndlessBehaviorMock();
            EndlessBehaviorMock notOptional = new EndlessBehaviorMock();
            BehaviorSequence sequence = new BehaviorSequence(false, new List<IBehavior>
            {
                notOptional,
                optional,
            });

            sequence.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);
            sequence.LifeCycle.Activate();

            yield return null;
            sequence.Update();

            // When the optional behavior is marked to be skipped before it was its turn,
            sequence.Configure(new Mode("Test", new WhitelistTypeRule<IOptional>().Add<OptionalEndlessBehaviorMock>()));

            notOptional.LifeCycle.MarkToFastForwardStage(Stage.Activating);
            notOptional.LifeCycle.MarkToFastForwardStage(Stage.Deactivating);

            while (notOptional.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                sequence.Update();
            }

            while (sequence.LifeCycle.Stage != Stage.Active)
            {
                Assert.AreEqual(Stage.Inactive, optional.LifeCycle.Stage);
                yield return null;
                sequence.Update();
            }

            // Then it is skipped.
            Assert.AreEqual(Stage.Active, sequence.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator SkipChildWhenItIsExecuted()
        {
            // Given an activating behavior sequence of one not optional and one optional behavior,
            OptionalEndlessBehaviorMock optional = new OptionalEndlessBehaviorMock();
            EndlessBehaviorMock notOptional = new EndlessBehaviorMock();
            BehaviorSequence sequence = new BehaviorSequence(false, new List<IBehavior>
            {
                notOptional,
                optional,
            });

            sequence.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            sequence.LifeCycle.Activate();

            yield return null;
            sequence.Update();

            notOptional.LifeCycle.MarkToFastForwardStage(Stage.Activating);
            notOptional.LifeCycle.MarkToFastForwardStage(Stage.Deactivating);

            // When the optional behavior is marked to be skipped when it is activating,
            sequence.Configure(new Mode("Test", new WhitelistTypeRule<IOptional>().Add<OptionalEndlessBehaviorMock>()));

            while (sequence.LifeCycle.Stage != Stage.Active)
            {
                Assert.AreEqual(Stage.Inactive, optional.LifeCycle.Stage);
                yield return null;
                sequence.Update();
            }

            // Then it is skipped.
            Assert.AreEqual(Stage.Active, sequence.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator UnskipChild()
        {
            // Given an activating repeating behavior sequence of one not optional and one skipped optional behavior,
            OptionalEndlessBehaviorMock optional = new OptionalEndlessBehaviorMock();
            EndlessBehaviorMock notOptional = new EndlessBehaviorMock();
            BehaviorSequence sequence = new BehaviorSequence(true, new List<IBehavior>
            {
                notOptional,
                optional,
            });

            sequence.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            sequence.LifeCycle.Activate();

            yield return null;
            sequence.Update();

            notOptional.LifeCycle.MarkToFastForwardStage(Stage.Activating);
            notOptional.LifeCycle.MarkToFastForwardStage(Stage.Deactivating);

            sequence.Configure(new Mode("Test", new WhitelistTypeRule<IOptional>().Add<OptionalEndlessBehaviorMock>()));

            yield return null;
            sequence.Update();

            //When you re-enable it,
            sequence.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            notOptional.LifeCycle.MarkToFastForwardStage(Stage.Activating);
            notOptional.LifeCycle.MarkToFastForwardStage(Stage.Deactivating);

            while (optional.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                sequence.Update();
            }

            // Then it is not skipped when it's its turn.
            Assert.AreEqual(Stage.Activating, optional.LifeCycle.Stage);
        }
    }
}
