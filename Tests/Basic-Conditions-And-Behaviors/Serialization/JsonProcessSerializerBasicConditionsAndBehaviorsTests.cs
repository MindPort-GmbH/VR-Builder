using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using VRBuilder.Core.Audio;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.EntityOwners;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Tests.RuntimeUtils;
using VRBuilder.Core.Tests.Utils;
using VRBuilder.Core.Tests.Utils.Builders;
using VRBuilder.Core.Tests.Utils.Mocks;

namespace VRBuilder.Core.Tests.Serialization
{
    public class JsonProcessSerializerBasicConditionsAndBehaviorsTests : RuntimeTests
    {
        [UnityTest]
        public IEnumerator ObjectInRangeCondition()
        {
            // Given a process with ObjectInRangeCondition,
            ProcessSceneObject testObjectToo = TestingUtils.CreateSceneObject("TestObjectToo");
            TransformInRangeDetectorProperty detector = testObjectToo.gameObject.AddComponent<TransformInRangeDetectorProperty>();
            ProcessSceneObject testObject = TestingUtils.CreateSceneObject("TestObject");

            IProcess process1 = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddCondition(new ObjectInRangeCondition(testObject, detector, 1.5f))))
                .Build();

            // When we serialize and deserialize it
            IProcess process2 = Serializer.ProcessFromByteArray(Serializer.ProcessToByteArray(process1));

            // Then that condition's target, detector and range should stay unchanged.
            ObjectInRangeCondition condition1 = process1.Data.FirstChapter.Data.FirstStep.Data.Transitions.Data.Transitions.First().Data.Conditions.First() as ObjectInRangeCondition;
            ObjectInRangeCondition condition2 = process2.Data.FirstChapter.Data.FirstStep.Data.Transitions.Data.Transitions.First().Data.Conditions.First() as ObjectInRangeCondition;

            Assert.IsNotNull(condition1);
            Assert.IsNotNull(condition2);
            Assert.AreEqual(condition1.Data.Range, condition2.Data.Range);
            Assert.AreEqual(condition1.Data.TargetObject.Value, condition2.Data.TargetObject.Value);
            Assert.AreEqual(condition1.Data.ReferenceObject.Value, condition2.Data.ReferenceObject.Value);

            // Cleanup
            TestingUtils.DestroySceneObject(testObjectToo);
            TestingUtils.DestroySceneObject(testObject);

            return null;
        }

        [UnityTest]
        public IEnumerator TimeoutCondition()
        {
            // Given a process with a timeout condition
            IProcess process1 = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddCondition(new TimeoutCondition(2.5f))))
                .Build();

            // When we serialize and deserialize it
            IProcess process2 = Serializer.ProcessFromByteArray((Serializer.ProcessToByteArray(process1)));

            // Then that condition's timeout value should stay unchanged.
            TimeoutCondition condition1 = process1.Data.FirstChapter.Data.FirstStep.Data.Transitions.Data.Transitions.First().Data.Conditions.First() as TimeoutCondition;
            TimeoutCondition condition2 = process2.Data.FirstChapter.Data.FirstStep.Data.Transitions.Data.Transitions.First().Data.Conditions.First() as TimeoutCondition;

            Assert.IsNotNull(condition1);
            Assert.IsNotNull(condition2);
            Assert.AreEqual(condition1.Data.Timeout, condition2.Data.Timeout);

            return null;
        }

        [UnityTest]
        public IEnumerator MoveObjectBehavior()
        {
            // Given process with MoveObjectBehavior
            ProcessSceneObject moved = TestingUtils.CreateSceneObject("moved");
            ProcessSceneObject positionProvider = TestingUtils.CreateSceneObject("positionprovider");
            IProcess process1 = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddBehavior(new MoveObjectBehavior(moved, positionProvider, 24.7f))))
                .Build();

            // When that process is serialized and deserialzied
            IProcess process2 = Serializer.ProcessFromByteArray(Serializer.ProcessToByteArray(process1));

            // Then we should have two identical move object behaviors
            MoveObjectBehavior behavior1 = process1.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First() as MoveObjectBehavior;
            MoveObjectBehavior behavior2 = process2.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First() as MoveObjectBehavior;

            Assert.IsNotNull(behavior1);
            Assert.IsNotNull(behavior2);
            Assert.IsFalse(ReferenceEquals(behavior1, behavior2));
            Assert.AreEqual(behavior1.Data.TargetObject.Value, behavior2.Data.TargetObject.Value);
            Assert.AreEqual(behavior1.Data.FinalPosition.Value, behavior2.Data.FinalPosition.Value);
            Assert.AreEqual(behavior1.Data.Duration, behavior2.Data.Duration);

            // Cleanup created game objects.
            TestingUtils.DestroySceneObject(moved);
            TestingUtils.DestroySceneObject(positionProvider);

            return null;
        }

        [UnityTest]
        public IEnumerator BehaviorSequence()
        {
            // Given a process with a behaviors sequence
            BehaviorSequence sequence = new BehaviorSequence(true, new List<IBehavior>
            {
                new DelayBehavior(0f),
                new EmptyBehaviorMock()
            });
            IProcess process = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddBehavior(sequence)))
                .Build();

            // When we serialize and deserialize it
            IProcess deserializedProcess = Serializer.ProcessFromByteArray(Serializer.ProcessToByteArray(process));

            BehaviorSequence deserializedSequence = deserializedProcess.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First() as BehaviorSequence;

            // Then the values stay the same.
            Assert.IsNotNull(deserializedSequence);
            Assert.AreEqual(sequence.Data.PlaysOnRepeat, deserializedSequence.Data.PlaysOnRepeat);

            List<IBehavior> behaviors = sequence.Data.Behaviors;
            List<IBehavior> deserializedBehaviors = deserializedSequence.Data.Behaviors;
            Assert.AreEqual(behaviors.First().GetType(), deserializedBehaviors.First().GetType());
            Assert.AreEqual(behaviors.Last().GetType(), deserializedBehaviors.Last().GetType());
            Assert.AreEqual(behaviors.Count, deserializedBehaviors.Count);
            yield break;
        }

        [UnityTest]
        public IEnumerator DelayBehavior()
        {
            // Given we have a process with a delayed activation behavior,
            IProcess process1 = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddBehavior(new DelayBehavior(7f))))
                .Build();

            // When we serialize and deserialize it,
            byte[] serialized = Serializer.ProcessToByteArray(process1);
            IProcess process2 = Serializer.ProcessFromByteArray(serialized);

            // Then that delayed behaviors should have the same target behaviors and delay time.
            DelayBehavior behavior1 = process1.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First() as DelayBehavior;
            DelayBehavior behavior2 = process2.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First() as DelayBehavior;

            Assert.AreEqual(behavior1.Data.DelayTime, behavior2.Data.DelayTime);

            return null;
        }

        [UnityTest]
        public IEnumerator PlayAudioOnActivationBehavior()
        {
            // Given a process with PlayAudioOnActivationBehavior with some ResourceAudio
            IProcess process1 = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddBehavior(new PlayAudioBehavior(new ResourceAudio("TestPath"), BehaviorExecutionStages.Activation))))
                .Build();

            // When we serialize and deserialize it,
            IProcess process2 = Serializer.ProcessFromByteArray(Serializer.ProcessToByteArray(process1));

            // Then path to the audiofile should not change.
            PlayAudioBehavior behavior1 = process1.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First() as PlayAudioBehavior;
            PlayAudioBehavior behavior2 = process2.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First() as PlayAudioBehavior;

            Assert.IsNotNull(behavior1);
            Assert.IsNotNull(behavior2);
            Assert.AreEqual(TestingUtils.GetField<string>(behavior1.Data.AudioData, "path"), TestingUtils.GetField<string>(behavior2.Data.AudioData, "path"));

            return null;
        }

        [UnityTest]
        public IEnumerator PlayAudioOnDectivationBehavior()
        {
            // Given a process with PlayAudioOnDeactivationBehavior and some ResourceData,
            IProcess process1 = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddBehavior(new PlayAudioBehavior(new ResourceAudio("TestPath"), BehaviorExecutionStages.Activation))))
                .Build();

            // When we serialize and deserialize it,
            IProcess process2 = Serializer.ProcessFromByteArray(Serializer.ProcessToByteArray(process1));

            PlayAudioBehavior behavior1 = process1.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First() as PlayAudioBehavior;
            PlayAudioBehavior behavior2 = process2.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First() as PlayAudioBehavior;

            // Then path to audio file should not change.
            Assert.IsNotNull(behavior1);
            Assert.IsNotNull(behavior2);
            Assert.AreEqual(TestingUtils.GetField<string>(behavior1.Data.AudioData, "path"), TestingUtils.GetField<string>(behavior2.Data.AudioData, "path"));

            return null;
        }

        [UnityTest]
        public IEnumerator ResourceAudio()
        {
            // Given we have a ResourceAudio instance,
            ResourceAudio audio = new ResourceAudio("TestPath");

            IProcess process = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddBehavior(new PlayAudioBehavior(audio, BehaviorExecutionStages.Activation))))
                .Build();

            // When we serialize and deserialize a process with it
            IProcess testProcess = Serializer.ProcessFromByteArray(Serializer.ProcessToByteArray(process));

            // Then the path to audio resource should be the same.
            string audioPath1 = TestingUtils.GetField<string>(((PlayAudioBehavior)process.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First()).Data.AudioData, "path");
            string audioPath2 = TestingUtils.GetField<string>(((PlayAudioBehavior)testProcess.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First()).Data.AudioData, "path");

            Assert.AreEqual(audioPath1, audioPath2);

            return null;
        }

        [UnityTest]
        public IEnumerator ExecuteChapters()
        {
            // Given an ExecuteChapters behavior
            IChapter parallelPath1 = new LinearChapterBuilder("Path 1")
                .AddStep(new BasicStepBuilder("Step 1"))
                .Build();

            IChapter parallelPath2 = new LinearChapterBuilder("Path 2")
                .AddStep(new BasicStepBuilder("Step 2"))
                .Build();
            IProcess process = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddBehavior(new ExecuteChaptersBehavior(new[] { parallelPath1, parallelPath2 }))))
                .Build();

            // When we serialize and deserialize a process with it
            IProcess testProcess = Serializer.ProcessFromByteArray(Serializer.ProcessToByteArray(process));

            // Then the sub-chapters are present.
            EntityCollectionData<IChapter> data = testProcess.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First(behavior => behavior is ExecuteChaptersBehavior).Data as EntityCollectionData<IChapter>;
            Assert.IsNotNull(data);
            Assert.AreEqual(data.GetChildren().Count(), 2);

            IChapter path1 = data.GetChildren().FirstOrDefault(chapter => chapter.Data.Name == "Path 1");
            Assert.IsNotNull(path1);
            Assert.AreEqual(path1.ChapterMetadata.Guid, parallelPath1.ChapterMetadata.Guid);
            IStep step1 = path1.Data.FirstStep;
            Assert.IsNotNull(step1);
            Assert.AreEqual(step1.StepMetadata.Guid, parallelPath1.Data.FirstStep.StepMetadata.Guid);

            IChapter path2 = data.GetChildren().FirstOrDefault(chapter => chapter.Data.Name == "Path 2");
            Assert.IsNotNull(path2);
            Assert.AreEqual(path2.ChapterMetadata.Guid, parallelPath2.ChapterMetadata.Guid);
            IStep step2 = path2.Data.FirstStep;
            Assert.IsNotNull(step2);
            Assert.AreEqual(step2.StepMetadata.Guid, parallelPath2.Data.FirstStep.StepMetadata.Guid);
            return null;
        }
    }
}
