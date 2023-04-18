using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core;
using VRBuilder.Tests.Utils;
using VRBuilder.XRInteraction.Properties;
using VRBuilder.BasicInteraction.Conditions;

namespace VRBuilder.XRInteraction.Tests.Conditions
{
    public class ReleasedConditionTests : RuntimeTests
    {
        private class GrabbablePropertyMock : GrabbableProperty
        {
            private bool isGrabbed = true;
            public override bool IsGrabbed
            {
                get
                {
                    return isGrabbed;
                }
            }

            public void SetGrabbed(bool value)
            {
                isGrabbed = value;
            }

            public void EmitUngrabEvent()
            {
                EmitUngrabbed();
            }
        }

        [UnityTest]
        public IEnumerator CompleteWhenUngrabbed()
        {
            // Setup object with mocked grabbed property and activate
            GameObject obj = new GameObject("T1");
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            GrabbablePropertyMock mockedProperty = obj.AddComponent<GrabbablePropertyMock>();

            yield return new WaitForFixedUpdate();

            ReleasedCondition condition = new ReleasedCondition(mockedProperty);
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // When it is ungrabbed
            mockedProperty.SetGrabbed(false);

            yield return null;
            condition.Update();

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted);

            // Clean up
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        public IEnumerator CompleteWhenItIsDoneOnStart()
        {
            // Setup object with mocked grabbed property and activate
            GameObject obj = new GameObject("T1");
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            GrabbablePropertyMock mockedProperty = obj.AddComponent<GrabbablePropertyMock>();
            mockedProperty.SetGrabbed(false);

            yield return new WaitForFixedUpdate();

            ReleasedCondition condition = new ReleasedCondition(mockedProperty);
            condition.LifeCycle.Activate();

            while (condition.IsCompleted == false)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is now completed due IsUngrabbed being true
            Assert.IsTrue(condition.IsCompleted);

            // Clean up
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        public IEnumerator ConditionNotCompleted()
        {
            // Setup object with mocked grabbed property and activate
            GameObject obj = new GameObject("T1");
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            GrabbablePropertyMock property = obj.AddComponent<GrabbablePropertyMock>();
            ReleasedCondition condition = new ReleasedCondition(property);
            condition.LifeCycle.Activate();

            float startTime = Time.time;
            while (Time.time < startTime + 0.1f)
            {
                yield return null;
                condition.Update();
            }

            // Assert after doing nothing the condition is not completed.
            Assert.IsFalse(condition.IsCompleted);

            // Clean up
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        public IEnumerator AutoCompleteActive()
        {
            // Given an ungrabbed condition
            GameObject go = new GameObject("Meme");
            go.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            GrabbablePropertyMock mock = go.AddComponent<GrabbablePropertyMock>();
            ReleasedCondition condition = new ReleasedCondition(mock);

            bool wasGrabbed = false;
            bool wasUngrabbed = false;
            mock.GrabStarted.AddListener((args) =>
            {
                wasGrabbed = true;
                mock.GrabEnded.AddListener((argsy) => wasUngrabbed = true);
            });

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
            Assert.IsTrue(condition.IsCompleted, "Condition should be complete!");
            Assert.IsTrue(wasGrabbed);
            Assert.IsTrue(wasUngrabbed);

            yield return null;
        }

        [UnityTest]
        public IEnumerator FastForwardDoesNotCompleteCondition()
        {
            // Given an ungrabbed condition
            GameObject go = new GameObject("Meme");
            go.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            GrabbablePropertyMock mock = go.AddComponent<GrabbablePropertyMock>();
            ReleasedCondition condition = new ReleasedCondition(mock);

            bool wasGrabbed = false;
            bool wasUngrabbed = false;
            mock.GrabStarted.AddListener((args) =>
            {
                wasGrabbed = true;
                mock.GrabEnded.AddListener((argsy) => wasUngrabbed = true);
            });

            // When you activate it,
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
            Assert.IsFalse(wasGrabbed);
            Assert.IsFalse(wasUngrabbed);

            yield return null;
        }
    }
}
