using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Core.Tests.RuntimeUtils;
using VRBuilder.Core.Tests.Utils;

namespace VRBuilder.Core.Tests.Behaviors
{
    public class SetObjectsEnabledBehaviorTests : RuntimeTests
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

        [UnityTest]
        public IEnumerator ObjectsAreEnabledAfterActivation()
        {
            // Given an active process object and a process with enable game object behavior,
            ProcessSceneObject toEnable1 = TestingUtils.CreateSceneObject("toEnable");
            toEnable1.AddTag(testTag);
            toEnable1.GameObject.SetActive(false);

            ProcessSceneObject toEnable2 = TestingUtils.CreateSceneObject("toEnable");
            toEnable2.AddTag(testTag);
            toEnable2.GameObject.SetActive(false);

            ProcessSceneObject toNotEnable1 = TestingUtils.CreateSceneObject("toEnable");
            toNotEnable1.GameObject.SetActive(false);

            SetObjectsEnabledBehavior behavior = new SetObjectsEnabledBehavior(testTag, true);

            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            // Then the process object is enabled.
            Assert.True(toEnable1.GameObject.activeSelf);
            Assert.True(toEnable2.GameObject.activeSelf);
            Assert.False(toNotEnable1.gameObject.activeSelf);

            // Cleanup
            TestingUtils.DestroySceneObject(toEnable1);
            TestingUtils.DestroySceneObject(toEnable2);
            TestingUtils.DestroySceneObject(toNotEnable1);

            yield break;
        }

        [UnityTest]
        public IEnumerator ObjectsStayEnabled()
        {
            ProcessSceneObject toEnable1 = TestingUtils.CreateSceneObject("toEnable");
            toEnable1.AddTag(testTag);
            toEnable1.GameObject.SetActive(false);

            ProcessSceneObject toEnable2 = TestingUtils.CreateSceneObject("toEnable");
            toEnable2.AddTag(testTag);
            toEnable2.GameObject.SetActive(false);

            ProcessSceneObject toNotEnable1 = TestingUtils.CreateSceneObject("toEnable");
            toNotEnable1.GameObject.SetActive(false);

            SetObjectsEnabledBehavior behavior = new SetObjectsEnabledBehavior(testTag, true);

            // When the behavior is activated and after the step is completed
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            behavior.LifeCycle.Deactivate();

            while (behavior.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                behavior.Update();
            }

            // Then the process object is enabled.
            Assert.True(toEnable1.GameObject.activeSelf);
            Assert.True(toEnable2.GameObject.activeSelf);
            Assert.False(toNotEnable1.gameObject.activeSelf);

            // Cleanup
            TestingUtils.DestroySceneObject(toEnable1);
            TestingUtils.DestroySceneObject(toEnable2);
            TestingUtils.DestroySceneObject(toNotEnable1);
        }

        [UnityTest]
        public IEnumerator ObjectsAreDisabledAfterActivation()
        {
            // Given an active process object and a process with disable game object behavior,
            ProcessSceneObject toDisable1 = TestingUtils.CreateSceneObject("toEnable");
            toDisable1.AddTag(testTag);

            ProcessSceneObject toDisable2 = TestingUtils.CreateSceneObject("toEnable");
            toDisable2.AddTag(testTag);

            ProcessSceneObject toNotDisable1 = TestingUtils.CreateSceneObject("toEnable");

            SetObjectsEnabledBehavior behavior = new SetObjectsEnabledBehavior(testTag, false);

            // When the behavior is activated
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            // Then the process object is disabled.
            Assert.False(toDisable1.GameObject.activeSelf);
            Assert.False(toDisable2.GameObject.activeSelf);
            Assert.True(toNotDisable1.gameObject.activeSelf);

            // Cleanup
            TestingUtils.DestroySceneObject(toDisable1);
            TestingUtils.DestroySceneObject(toDisable2);
            TestingUtils.DestroySceneObject(toNotDisable1);

            yield break;
        }

        [UnityTest]
        public IEnumerator ObjectsStayDisabled()
        {
            // Given an active process object and a process with disable game object behavior,
            ProcessSceneObject toDisable1 = TestingUtils.CreateSceneObject("toEnable");
            toDisable1.AddTag(testTag);

            ProcessSceneObject toDisable2 = TestingUtils.CreateSceneObject("toEnable");
            toDisable2.AddTag(testTag);

            ProcessSceneObject toNotDisable1 = TestingUtils.CreateSceneObject("toEnable");

            SetObjectsEnabledBehavior behavior = new SetObjectsEnabledBehavior(testTag, false);

            // When the behavior is activated and after the step is completed
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            // Then the process object stays disabled.
            Assert.False(toDisable1.GameObject.activeSelf);
            Assert.False(toDisable2.GameObject.activeSelf);
            Assert.True(toNotDisable1.gameObject.activeSelf);

            // Cleanup
            TestingUtils.DestroySceneObject(toDisable1);
            TestingUtils.DestroySceneObject(toDisable2);
            TestingUtils.DestroySceneObject(toNotDisable1);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehavior()
        {
            ProcessSceneObject toEnable1 = TestingUtils.CreateSceneObject("toEnable");
            toEnable1.AddTag(testTag);
            toEnable1.GameObject.SetActive(false);

            ProcessSceneObject toEnable2 = TestingUtils.CreateSceneObject("toEnable");
            toEnable2.AddTag(testTag);
            toEnable2.GameObject.SetActive(false);

            ProcessSceneObject toNotEnable1 = TestingUtils.CreateSceneObject("toEnable");
            toNotEnable1.GameObject.SetActive(false);

            SetObjectsEnabledBehavior behavior = new SetObjectsEnabledBehavior(testTag, true);

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it doesn't autocomplete because it weren't activated yet.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);

            // Cleanup.
            TestingUtils.DestroySceneObject(toEnable1);
            TestingUtils.DestroySceneObject(toEnable2);
            TestingUtils.DestroySceneObject(toNotEnable1);
            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given an inactive process object and a EnableGameObjectBehavior,
            ProcessSceneObject toEnable1 = TestingUtils.CreateSceneObject("toEnable");
            toEnable1.AddTag(testTag);
            toEnable1.GameObject.SetActive(false);

            ProcessSceneObject toEnable2 = TestingUtils.CreateSceneObject("toEnable");
            toEnable2.AddTag(testTag);
            toEnable2.GameObject.SetActive(false);

            ProcessSceneObject toNotEnable1 = TestingUtils.CreateSceneObject("toEnable");
            toNotEnable1.GameObject.SetActive(false);

            SetObjectsEnabledBehavior behavior = new SetObjectsEnabledBehavior(testTag, true);

            // When we mark it to fast-forward and activate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            // Then it should work without any differences because the behavior is done immediately anyways.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.True(toEnable1.GameObject.activeSelf);
            Assert.True(toEnable2.GameObject.activeSelf);
            Assert.False(toNotEnable1.gameObject.activeSelf);

            // Cleanup.
            TestingUtils.DestroySceneObject(toEnable1);
            TestingUtils.DestroySceneObject(toEnable2);
            TestingUtils.DestroySceneObject(toNotEnable1);
            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBehavior()
        {
            // Given an inactive process object and an active EnableGameObjectBehavior,
            ProcessSceneObject toEnable1 = TestingUtils.CreateSceneObject("toEnable");
            toEnable1.AddTag(testTag);
            toEnable1.GameObject.SetActive(false);

            ProcessSceneObject toEnable2 = TestingUtils.CreateSceneObject("toEnable");
            toEnable2.AddTag(testTag);
            toEnable2.GameObject.SetActive(false);

            ProcessSceneObject toNotEnable1 = TestingUtils.CreateSceneObject("toEnable");
            toNotEnable1.GameObject.SetActive(false);

            SetObjectsEnabledBehavior behavior = new SetObjectsEnabledBehavior(testTag, true);

            behavior.LifeCycle.Activate();

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it should work without any differences because the behavior is done immediately anyways.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.True(toEnable1.GameObject.activeSelf);
            Assert.True(toEnable2.GameObject.activeSelf);
            Assert.False(toNotEnable1.gameObject.activeSelf);

            // Cleanup.
            TestingUtils.DestroySceneObject(toEnable1);
            TestingUtils.DestroySceneObject(toEnable2);
            TestingUtils.DestroySceneObject(toNotEnable1);
            yield break;
        }
    }
}
