//using System.Collections;
//using VRBuilder.Core.Behaviors;
//using VRBuilder.Tests.Builder;
//using VRBuilder.Core.Configuration;
//using VRBuilder.Core.SceneObjects;
//using VRBuilder.Tests.Utils;
//using VRBuilder.Tests.Utils.Mocks;
//using UnityEngine;
//using UnityEngine.TestTools;
//using NUnit.Framework;

//namespace VRBuilder.Core.Tests.Behaviors
//{
//    public class DisableGameObjectBehaviorTests : RuntimeTests
//    {
//        [UnityTest]
//        public IEnumerator GameObjectIsDisabledAfterActivation()
//        {
//            // Given an active process object and a process with disable game object behavior,
//            ProcessSceneObject toDisable = TestingUtils.CreateSceneObject("ToDisable");
//            EndlessConditionMock trigger = new EndlessConditionMock();

//            BasicProcessStepBuilder basicStepBuilder = new BasicProcessStepBuilder("Step");

//            IProcess process = new LinearProcessBuilder("Process")
//                .AddChapter(new LinearChapterBuilder("Chapter")
//                    .AddStep(basicStepBuilder
//                        .Disable(toDisable)
//                        .AddCondition(trigger)))
//                .Build();

//            process.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

//            // When the behavior is activated
//            ProcessRunner.Initialize(process);
//            ProcessRunner.Run();

//            yield return new WaitUntil(()=> process.Data.FirstChapter.Data.Steps[0].LifeCycle.Stage == Stage.Active);

//            trigger.Autocomplete();

//            yield return new WaitUntil(()=> process.Data.FirstChapter.Data.Steps[0].LifeCycle.Stage == Stage.Inactive);

//            // Then the process object is disabled.
//            Assert.False(toDisable.GameObject.activeSelf);

//            // Cleanup.
//            TestingUtils.DestroySceneObject(toDisable);

//            yield break;
//        }

//        [UnityTest]
//        public IEnumerator GameObjectStaysDisabled()
//        {
//            // Given an active process object and a process with disable game object behavior,
//            ProcessSceneObject toDisable = TestingUtils.CreateSceneObject("ToDisable");
//            EndlessConditionMock trigger = new EndlessConditionMock();

//            IProcess process = new LinearProcessBuilder("Process")
//                .AddChapter(new LinearChapterBuilder("Chapter")
//                    .AddStep(new BasicProcessStepBuilder("Step")
//                        .Disable(toDisable))
//                    .AddStep(new BasicProcessStepBuilder("Step")
//                        .AddCondition(trigger)))
//                .Build();

//            process.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

//            // When the behavior is activated and after the step is completed
//            ProcessRunner.Initialize(process);
//            ProcessRunner.Run();

//            yield return new WaitUntil(()=> process.Data.FirstChapter.Data.Steps[1].LifeCycle.Stage == Stage.Active);

//            trigger.Autocomplete();

//            yield return new WaitUntil(()=> process.Data.FirstChapter.Data.Steps[1].LifeCycle.Stage == Stage.Inactive);

//            // Then the process object stays disabled.
//            Assert.False(toDisable.GameObject.activeSelf);

//            // Cleanup.
//            TestingUtils.DestroySceneObject(toDisable);

//            yield break;
//        }

//        [UnityTest]
//        public IEnumerator FastForwardInactiveBehavior()
//        {
//            // Given an active process object and a DisableGameObjectBehavior,
//            ProcessSceneObject toDisable = TestingUtils.CreateSceneObject("ToDisable");

//            DisableGameObjectBehavior behavior = new DisableGameObjectBehavior(toDisable);

//            // When we mark it to fast-forward,
//            behavior.LifeCycle.MarkToFastForward();

//            // Then it doesn't autocomplete because it weren't activated yet.
//            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);

//            // Cleanup.
//            TestingUtils.DestroySceneObject(toDisable);

//            yield break;
//        }

//        [UnityTest]
//        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
//        {
//            // Given an active process object and a DisableGameObjectBehavior,
//            ProcessSceneObject toDisable = TestingUtils.CreateSceneObject("ToDisable");

//            DisableGameObjectBehavior behavior = new DisableGameObjectBehavior(toDisable);

//            // When we mark it to fast-forward and activate it,
//            behavior.LifeCycle.MarkToFastForward();
//            behavior.LifeCycle.Activate();

//            // Then it should work without any differences because the behavior is done immediately anyways.
//            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
//            Assert.IsFalse(toDisable.GameObject.activeSelf);

//            // Cleanup.
//            TestingUtils.DestroySceneObject(toDisable);

//            yield break;
//        }

//        [UnityTest]
//        public IEnumerator FastForwardActivatingBehavior()
//        {
//            // Given an active process object and an active DisableGameObjectBehavior,
//            ProcessSceneObject toDisable = TestingUtils.CreateSceneObject("ToDisable");

//            DisableGameObjectBehavior behavior = new DisableGameObjectBehavior(toDisable);

//            behavior.LifeCycle.Activate();

//            // When we mark it to fast-forward,
//            behavior.LifeCycle.MarkToFastForward();

//            // Then it should work without any differences because the behavior is done immediately anyways.
//            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
//            Assert.IsFalse(toDisable.GameObject.activeSelf);

//            // Cleanup.
//            TestingUtils.DestroySceneObject(toDisable);

//            yield break;
//        }
//    }
//}
