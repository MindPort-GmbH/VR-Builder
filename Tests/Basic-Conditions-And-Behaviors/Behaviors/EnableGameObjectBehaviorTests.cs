using UnityEngine;
using System.Collections;
using VRBuilder.Core.Behaviors;
using VRBuilder.Tests.Builder;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Tests.Utils;
using VRBuilder.Tests.Utils.Mocks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace VRBuilder.Core.Tests.Behaviors
{
    public class EnableGameObjectBehaviorTests : RuntimeTests
    {
        [UnityTest]
        public IEnumerator GameObjectIsEnabledAfterActivation()
        {
            // Given an active process object and a process with enable game object behavior,
            ProcessSceneObject toEnable = TestingUtils.CreateSceneObject("toEnable");
            toEnable.GameObject.SetActive(false);

            EndlessConditionMock trigger = new EndlessConditionMock();

            IProcess process = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicProcessStepBuilder("Step")
                        .Enable(toEnable)
                        .AddCondition(trigger)))
                .Build();

            process.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            ProcessRunner.Initialize(process);
            ProcessRunner.Run();

            // When the behavior is activated
            ProcessRunner.Initialize(process);
            ProcessRunner.Run();

            yield return new WaitUntil(()=> process.Data.FirstChapter.Data.Steps[0].LifeCycle.Stage == Stage.Active);

            // Then the process object is enabled.
            Assert.True(toEnable.GameObject.activeSelf);

            // Cleanup
            TestingUtils.DestroySceneObject(toEnable);

            yield break;
        }

        [UnityTest]
        public IEnumerator GameObjectStaysEnabled()
        {
            // Given an active process object and a process with enalbe game object condition,
            ProcessSceneObject toEnable = TestingUtils.CreateSceneObject("toEnable");
            toEnable.GameObject.SetActive(false);

            EndlessConditionMock trigger = new EndlessConditionMock();

            IProcess process = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicProcessStepBuilder("Step")
                        .Enable(toEnable))
                    .AddStep(new BasicProcessStepBuilder("Step")
                        .AddCondition(trigger)))
                .Build();

            process.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When the behavior is activated and after the step is completed
            ProcessRunner.Initialize(process);
            ProcessRunner.Run();

            yield return new WaitUntil(()=> process.Data.FirstChapter.Data.Steps[0].LifeCycle.Stage == Stage.Active);

            trigger.Autocomplete();

            yield return new WaitUntil(()=> process.Data.FirstChapter.Data.Steps[0].LifeCycle.Stage == Stage.Inactive);

            // Then the process object stays enabled.
            Assert.True(toEnable.GameObject.activeSelf);

            // Cleanup
            TestingUtils.DestroySceneObject(toEnable);
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehavior()
        {
            // Given an inactive process object and an EnableGameObjectBehavior,
            ProcessSceneObject toEnable = TestingUtils.CreateSceneObject("ToEnable");
            toEnable.GameObject.SetActive(false);

            EnableGameObjectBehavior behavior = new EnableGameObjectBehavior(toEnable);

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it doesn't autocomplete because it weren't activated yet.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);

            // Cleanup.
            TestingUtils.DestroySceneObject(toEnable);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given an inactive process object and a EnableGameObjectBehavior,
            ProcessSceneObject toEnable = TestingUtils.CreateSceneObject("ToEnable");
            toEnable.GameObject.SetActive(false);

            EnableGameObjectBehavior behavior = new EnableGameObjectBehavior(toEnable);

            // When we mark it to fast-forward and activate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            // Then it should work without any differences because the behavior is done immediately anyways.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue(toEnable.GameObject.activeSelf);

            // Cleanup.
            TestingUtils.DestroySceneObject(toEnable);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBehavior()
        {
            // Given an inactive process object and an active EnableGameObjectBehavior,
            ProcessSceneObject toEnable = TestingUtils.CreateSceneObject("ToEnable");
            toEnable.GameObject.SetActive(false);

            EnableGameObjectBehavior behavior = new EnableGameObjectBehavior(toEnable);

            behavior.LifeCycle.Activate();

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it should work without any differences because the behavior is done immediately anyways.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue(toEnable.GameObject.activeSelf);

            // Cleanup.
            TestingUtils.DestroySceneObject(toEnable);

            yield break;
        }
    }
}
