using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core;
using VRBuilder.Tests.Utils;
using VRBuilder.Core.Configuration;
using VRBuilder.XRInteraction.Properties;
using VRBuilder.BasicInteraction.Conditions;
using VRBuilder.Core.Settings;
using System;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.XRInteraction.Tests.Conditions
{
    public class GrabbedByTagConditionTests : RuntimeTests
    {
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

        private class GrabbablePropertyMock : GrabbableProperty
        {
            private bool isGrabbed;

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

            public void EmitGrabEvent()
            {
                EmitGrabbed();
            }
        }

        [UnityTest]
        public IEnumerator CompleteWhenGrabbed()
        {
            // Setup object with mocked grabbed property and activate
            GameObject obj = new GameObject("T1");
            ProcessSceneObject sceneObject= obj.AddComponent<ProcessSceneObject>();
            sceneObject.AddTag(testTag);
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            GrabbablePropertyMock mockedProperty = obj.AddComponent<GrabbablePropertyMock>();

            yield return null;

            GrabbedObjectWithTagCondition condition = new GrabbedObjectWithTagCondition(testTag);
            condition.LifeCycle.Activate();

            yield return null;
            condition.Update();

            // Grab object
            mockedProperty.SetGrabbed(true);

            yield return null;
            condition.Update();

            // Assert that condition is now completed
            Assert.IsTrue(condition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator CompleteWhenGrabbedOnActivation()
        {
            // Setup object with mocked grabbed property and activate
            GameObject obj = new GameObject("T1");
            ProcessSceneObject sceneObject = obj.AddComponent<ProcessSceneObject>();
            sceneObject.AddTag(testTag);
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            GrabbablePropertyMock mockedProperty = obj.AddComponent<GrabbablePropertyMock>();
            mockedProperty.SetGrabbed(true);

            yield return null;

            GrabbedObjectWithTagCondition condition = new GrabbedObjectWithTagCondition(testTag);
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
            ProcessSceneObject sceneObject = obj.AddComponent<ProcessSceneObject>();
            sceneObject.AddTag(testTag);
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            GrabbablePropertyMock mockedProperty = obj.AddComponent<GrabbablePropertyMock>();

            GrabbedObjectWithTagCondition condition = new GrabbedObjectWithTagCondition(testTag);

            condition.LifeCycle.Activate();
            yield return null;

            // Assert after doing nothing the condition is not completed.
            Assert.IsFalse(condition.IsCompleted, "Condition should not be complete!");
        }

        [UnityTest]
        public IEnumerator AutoCompleteActive()
        {
            // Given a grabbed condition
            GameObject obj = new GameObject("T1");
            ProcessSceneObject sceneObject = obj.AddComponent<ProcessSceneObject>();
            sceneObject.AddTag(testTag);
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            GrabbablePropertyMock mockedProperty = obj.AddComponent<GrabbablePropertyMock>();
            GrabbedObjectWithTagCondition condition = new GrabbedObjectWithTagCondition(testTag);

            bool wasGrabbed = false;
            bool wasUngrabbed = false;

            mockedProperty.OnGrabbed.AddListener((args) =>
            {
                wasGrabbed = true;
                mockedProperty.OnUngrabbed.AddListener((argsy) => wasUngrabbed = true);
            });

            condition.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                condition.Update();
            }

            // When you mark it autocomplete,
            condition.Autocomplete();

            // Then it is completed, and the object was grabbed and immediately released.
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);
            Assert.IsTrue(condition.IsCompleted);
            Assert.IsTrue(wasGrabbed);
            Assert.IsTrue(wasUngrabbed);

            yield return null;
        }

        [UnityTest]
        public IEnumerator FastForwardDoesNotCompleteCondition()
        {
            // Given a condition,
            GameObject obj = new GameObject("T1");
            ProcessSceneObject sceneObject = obj.AddComponent<ProcessSceneObject>();
            sceneObject.AddTag(testTag);
            obj.AddComponent<TouchedConditionTests.TouchablePropertyMock>();
            GrabbablePropertyMock mockedProperty = obj.AddComponent<GrabbablePropertyMock>();
            GrabbedObjectWithTagCondition condition = new GrabbedObjectWithTagCondition(testTag);

            bool wasGrabbed = false;
            bool wasUngrabbed = false;

            mockedProperty.OnGrabbed.AddListener((args) =>
            {
                wasGrabbed = true;
                mockedProperty.OnUngrabbed.AddListener((argsy) => wasUngrabbed = true);
            });

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
        }
    }
}