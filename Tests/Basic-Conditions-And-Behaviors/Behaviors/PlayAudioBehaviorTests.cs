using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core.Audio;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Tests.RuntimeUtils;

namespace VRBuilder.Core.Tests.Behaviors
{
    public class PlayAudioBehaviorTests : RuntimeTests
    {
        private static AudioSource audioSource;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            // Setup the player and its audio source in here.
            // AudioSource.playOnAwake is by default true. Thus audioSource.isPlaying is true during the first frame.
            // The first frame is skipped after setup and audioSource.isPlaying is false as desired.
            GameObject player = new GameObject("AudioPlayer");
            audioSource = player.AddComponent<AudioSource>();
        }

        [UnityTest]
        public IEnumerator AudioIsPlayed()
        {
            // Given a PlayAudioBehavior,
            ResourceAudio audioData = new ResourceAudio("Sounds/test-sound");
            IBehavior behavior = new PlayAudioBehavior(audioData, BehaviorExecutionStages.Activation, audioSource);

            // When we activate it,
            behavior.LifeCycle.Activate();

            // Then audio is played.
            Assert.IsTrue(audioSource.isPlaying);

            yield break;
        }

        [UnityTest]
        public IEnumerator ActivatingWhileAudioPlays()
        {
            // Given a PlayAudioBehavior,
            ResourceAudio audioData = new ResourceAudio("Sounds/test-sound");
            IBehavior behavior = new PlayAudioBehavior(audioData, BehaviorExecutionStages.Activation, audioSource);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate it,
            behavior.LifeCycle.Activate();

            yield return null;
            behavior.Update();

            // Then that audio source is playing but behavior is active.
            Assert.IsTrue(audioSource.isPlaying);
            Assert.AreEqual(Stage.Activating, behavior.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator ActiveAfterAudioPlayed()
        {
            // Given a PlayAudioBehavior,
            ResourceAudio audioData = new ResourceAudio("Sounds/test-sound");
            IBehavior behavior = new PlayAudioBehavior(audioData, BehaviorExecutionStages.Activation, audioSource);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate it and wait for the audio to play back,
            behavior.LifeCycle.Activate();

            float startTime = Time.time;
            while (audioSource.isPlaying)
            {
                Assert.AreEqual(Stage.Activating, behavior.LifeCycle.Stage);
                yield return null;
                behavior.Update();
            }

            float duration = Time.time - startTime;

            // Then the audio is not playing and the behavior is active.
            Assert.AreEqual(audioData.AudioClip.length, duration, 0.1f);
            Assert.IsFalse(audioSource.isPlaying);
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator AudioIsPlayedOnDeactivation()
        {
            // Given a PlayAudioBehavior with activation mode "Deactivation",
            ResourceAudio audioData = new ResourceAudio("Sounds/test-sound");
            IBehavior behavior = new PlayAudioBehavior(audioData, BehaviorExecutionStages.Deactivation, audioSource);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate and deactivate it,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            behavior.LifeCycle.Deactivate();

            yield return null;
            behavior.Update();

            // Then the audio is playing.
            Assert.IsTrue(audioSource.isPlaying);
        }

        [UnityTest]
        public IEnumerator StillDeactivatingWhenPlayingAudio()
        {
            // Given a PlayAudioBehavior with activation mode "Deactivation",
            ResourceAudio audioData = new ResourceAudio("Sounds/test-sound");
            IBehavior behavior = new PlayAudioBehavior(audioData, BehaviorExecutionStages.Deactivation, audioSource);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate and deactivate it,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            behavior.LifeCycle.Deactivate();

            yield return null;
            behavior.Update();

            // Then that audio source is playing but behavior is deactivating.
            Assert.IsTrue(audioSource.isPlaying);
            Assert.AreEqual(Stage.Deactivating, behavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator IsDeactivatedAfterPlayingAudio()
        {
            // Given a PlayAudioBehavior with activation mode "Deactivation",
            ResourceAudio audioData = new ResourceAudio("Sounds/test-sound");
            IBehavior behavior = new PlayAudioBehavior(audioData, BehaviorExecutionStages.Deactivation, audioSource);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate and deactivate it and wait until the clip stops playing,
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            behavior.LifeCycle.Deactivate();

            float startTime = Time.time;
            while (audioSource.isPlaying)
            {
                yield return null;
                behavior.Update();
            }

            float duration = Time.time - startTime;

            // Then the behavior is deactivated after the clip's duration has elapsed, within a margin of error.
            Assert.AreEqual(audioData.AudioClip.length, duration, 0.1f);
            Assert.IsFalse(audioSource.isPlaying);
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator BehaviorWithoutAudioClipIsActivatedImmediately()
        {
            // Given a PlayAudioBehavior with empty audio data and mode set to Activation
            ResourceAudio audioData = new ResourceAudio(null);
            IBehavior behavior = new PlayAudioBehavior(audioData, BehaviorExecutionStages.Activation, audioSource);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When behavior is activated
            behavior.LifeCycle.Activate();

            yield return null;
            behavior.Update();

            // Then it immediately finishes its activation.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator BehaviorWithoutAudioClipIsDeactivatedImmediately()
        {
            // Given a PlayAudioBehavior with empty audio data and mode set to Deactivation
            ResourceAudio audioData = new ResourceAudio(null);
            IBehavior behavior = new PlayAudioBehavior(audioData, BehaviorExecutionStages.Deactivation, audioSource);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);


            // When behavior is activated
            behavior.LifeCycle.Activate();

            yield return null;
            behavior.Update();

            behavior.LifeCycle.Deactivate();

            yield return null;
            behavior.Update();

            // Then it immediately finishes its activation.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehavior()
        {
            // Given a PlayAudioBehavior with activation mode "Activation",
            ResourceAudio audioData = new ResourceAudio("Sounds/test-sound");
            IBehavior behavior = new PlayAudioBehavior(audioData, BehaviorExecutionStages.Activation, audioSource);

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it doesn't autocomplete because it hasn't been activated yet.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given a PlayAudioBehavior with activation mode "Activation",
            ResourceAudio audioData = new ResourceAudio("Sounds/test-sound");
            IBehavior behavior = new PlayAudioBehavior(audioData, BehaviorExecutionStages.Activation, audioSource);

            // When we mark it to fast-forward and activate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            // Then it autocompletes immediately and audio is not playing.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsFalse(audioSource.isPlaying);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndDeactivateIt()
        {
            // Given a PlayAudioBehavior with activation mode "Deactivation",
            ResourceAudio audioData = new ResourceAudio("Sounds/test-sound");
            IBehavior behavior = new PlayAudioBehavior(audioData, BehaviorExecutionStages.Deactivation, audioSource);

            // When we mark it to fast-forward, activate and immediately deactivate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();
            behavior.LifeCycle.Deactivate();

            // Then it autocompletes immediately and audio is not playing.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
            Assert.IsFalse(audioSource.isPlaying);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBehavior()
        {
            // Given an active PlayAudioBehavior with activation mode "Activation",
            ResourceAudio audioData = new ResourceAudio("Sounds/test-sound");
            IBehavior behavior = new PlayAudioBehavior(audioData, BehaviorExecutionStages.Activation, audioSource);

            behavior.LifeCycle.Activate();

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it autocompletes immediately and audio is not playing.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsFalse(audioSource.isPlaying);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardDeactivatingBehavior()
        {
            // Given a deactivating PlayAudioBehavior with activation mode "Activation",
            ResourceAudio audioData = new ResourceAudio("Sounds/test-sound");
            IBehavior behavior = new PlayAudioBehavior(audioData, BehaviorExecutionStages.Deactivation, audioSource);

            behavior.LifeCycle.Activate();
            behavior.LifeCycle.Deactivate();

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it autocompletes immediately and audio is not playing.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
            Assert.IsFalse(audioSource.isPlaying);

            yield break;
        }
    }
}
