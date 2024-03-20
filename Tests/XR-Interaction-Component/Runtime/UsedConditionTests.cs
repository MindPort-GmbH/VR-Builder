using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.BasicInteraction.Conditions;
using VRBuilder.Core;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Core.Tests.RuntimeUtils;
using VRBuilder.XRInteraction.Properties;

namespace VRBuilder.XRInteraction.Tests.Conditions
{
    public class UsedConditionTests : RuntimeTests
    {
        private class UsablePropertyMock : UsableProperty
        {
            private bool isUsed;
            public override bool IsBeingUsed
            {
                get
                {
                    return isUsed;
                }
            }

            public void SetUsed(bool value)
            {
                isUsed = value;
            }

            public void EmitIsUsed()
            {
                EmitUsageStarted();
            }
        }

        private Guid testTag;

        [SetUp]
        public void CreateTestTags()
        {
            testTag = (SceneObjectTags.Instance.CreateTag("unit test tag, delete me please", Guid.NewGuid()).Guid);
        }

        [TearDown]
        public void RemoveTestTags()
        {
            SceneObjectTags.Instance.RemoveTag(testTag);
            testTag = Guid.Empty;
        }

        [UnityTest]
        public IEnumerator CompleteWhenUsed()
        {
            // Setup object with mocked grabbed property and activate
            GameObject obj = new GameObject("T1");
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            UsablePropertyMock mockedProperty = obj.AddComponent<UsablePropertyMock>();

            yield return null;

            UsedCondition condition = new UsedCondition(mockedProperty);
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // When it is used
            mockedProperty.SetUsed(true);

            yield return null;
            condition.Update();

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted);
        }


        [UnityTest]
        public IEnumerator CompleteWhenUsedByTag()
        {
            // Setup object with mocked grabbed property and activate
            GameObject obj = new GameObject("T1");
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            UsablePropertyMock mockedProperty = obj.AddComponent<UsablePropertyMock>();
            obj.GetComponent<ProcessSceneObject>().AddTag(testTag);

            GameObject obj2 = new GameObject("T2");
            obj2.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            obj2.AddComponent<UsablePropertyMock>();
            obj2.GetComponent<ProcessSceneObject>().AddTag(testTag);

            yield return null;

            UsedCondition condition = new UsedCondition(testTag);
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // When it is used
            mockedProperty.SetUsed(true);

            yield return null;
            condition.Update();

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator CompleteWhenItIsDoneOnStart()
        {
            // Setup object with mocked grabbed property and activate
            GameObject obj = new GameObject("T1");
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            UsablePropertyMock mockedProperty = obj.AddComponent<UsablePropertyMock>();
            mockedProperty.SetUsed(true);

            yield return null;

            UsedCondition condition = new UsedCondition(mockedProperty);
            condition.LifeCycle.Activate();

            while (condition.IsCompleted == false)
            {
                yield return null;
                condition.Update();
            }

            // Assert that condition is now completed due IsGrabbed is true
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator ConditionNotCompleted()
        {
            // Setup object with mocked grabbed property and activate
            GameObject obj = new GameObject("T1");
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            UsableProperty usableProperty = obj.AddComponent<UsablePropertyMock>();
            UsedCondition condition = new UsedCondition(usableProperty);
            condition.LifeCycle.Activate();

            float startTime = Time.time;
            while (Time.time < startTime + 0.1f)
            {
                yield return null;
                condition.Update();
            }

            // Assert after doing nothing the condition is not completed.
            Assert.IsFalse(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator AutoCompleteActive()
        {
            // Given an used condition,
            GameObject obj = new GameObject("T1");
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            UsablePropertyMock mockedProperty = obj.AddComponent<UsablePropertyMock>();

            bool wasUsageStarted = false;
            bool wasUsageStopped = false;

            mockedProperty.UseStarted.AddListener((args) =>
            {
                wasUsageStarted = true;
                mockedProperty.UseEnded.AddListener((args) => wasUsageStopped = true);
            });

            UsedCondition condition = new UsedCondition(mockedProperty);

            // When you activate and autocomplete it,
            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            condition.Autocomplete();

            // Then condition is completed.
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);
            Assert.IsTrue(condition.IsCompleted);
            Assert.IsTrue(wasUsageStarted);
            Assert.IsTrue(wasUsageStopped);
        }

        [UnityTest]
        public IEnumerator FastForwardDoesNotCompleteCondition()
        {
            // Given an used condition,
            GameObject obj = new GameObject("T1");
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            UsablePropertyMock mockedProperty = obj.AddComponent<UsablePropertyMock>();

            bool wasUsageStarted = false;
            bool wasUsageStopped = false;

            mockedProperty.UseStarted.AddListener((args) =>
            {
                wasUsageStarted = true;
                mockedProperty.UseEnded.AddListener((args) => wasUsageStopped = true);
            });

            UsedCondition condition = new UsedCondition(mockedProperty);

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
            Assert.IsFalse(wasUsageStarted);
            Assert.IsFalse(wasUsageStopped);
        }
    }
}
