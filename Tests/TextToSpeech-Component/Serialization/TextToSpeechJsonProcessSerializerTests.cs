using System.Linq;
using System.Collections;
using Innoactive.Creator.Core.Tests.Utils;
using Innoactive.Hub.Training;
using Innoactive.Hub.Training.Audio;
using Innoactive.Hub.Training.Behaviors;
using Innoactive.Hub.Training.Utils.Builders;
using Innoactive.Creator.Internationalization;
using UnityEngine.TestTools;
using UnityEngine.Assertions;

namespace Innoactive.Hub.Unity.Tests.Training
{
    public class TextToSpeechJsonProcessSerializerTests : RuntimeTests
    {
        [UnityTest]
        public IEnumerator TextToSpeechAudio()
        {
            // Given we have TextToSpeechAudio instance,
            TextToSpeechAudio audio = new TextToSpeechAudio(new LocalizedString("TestPath"));

            ICourse course = new LinearTrainingBuilder("Training")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .DisableAutomaticAudioHandling()
                        .AddBehavior(new PlayAudioBehavior(audio, BehaviorExecutionStages.Activation))))
                .Build();

            // When we serialize and deserialize a training with it,
            ICourse testCourse =  Serializer.ToCourse(Serializer.ToByte(course));

            // Then the text to generate sound from should be the same.
            IAudioData data1 = ((PlayAudioBehavior)course.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First()).Data.AudioData;
            IAudioData data2 = ((PlayAudioBehavior)testCourse.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First()).Data.AudioData;

            string audioPath1 = TestingUtils.GetField<LocalizedString>(data1, "text").Key;
            string audioPath2 = TestingUtils.GetField<LocalizedString>(data2, "text").Key;

            Assert.AreEqual(data1.GetType(), data2.GetType());

            Assert.AreEqual(audioPath1, "TestPath");
            Assert.AreEqual(audioPath1, audioPath2);

            return null;
        }
    }
}