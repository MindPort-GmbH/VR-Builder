using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Audio;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Configuration.Modes;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// A behavior that plays audio.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-behaviors/play-audio-file")]
    public class PlayAudioBehavior : Behavior<PlayAudioBehavior.EntityData>, IOptional
    {
        /// <summary>
        /// The "play audio" behavior's data.
        /// </summary>
        [DataContract(IsReference = true)]
        public class EntityData : IBackgroundBehaviorData
        {
            /// <summary>
            /// An audio data that contains an audio clip to play.
            /// </summary>
            [DataMember]
            public IAudioData AudioData { get; set; }

            /// <summary>
            /// A property that determines if the audio should be played at activation or deactivation (or both).
            /// </summary>
            [DataMember]
            [DisplayName("Execution stages")]
            public BehaviorExecutionStages ExecutionStages { get; set; }

            /// <summary>
            /// Audio volume this audio file should be played with.
            /// </summary>
            [DataMember]
            [DisplayName("Audio Volume (from 0 to 1)")]            
            public float Volume { get; set; } = 1.0f;

            /// <summary>
            /// The Unity's audio source to play the sound. If not set, it will use <seealso cref="RuntimeConfigurator.Configuration.InstructionPlayer"/>.
            /// </summary>
            public AudioSource AudioPlayer { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            public string Name { get; set; }

            /// <inheritdoc />
            public bool IsBlocking { get; set; }
        }

        private class PlayAudioProcess : StageProcess<EntityData>
        {
            private readonly BehaviorExecutionStages executionStages;

            public PlayAudioProcess(BehaviorExecutionStages executionStages, EntityData data) : base(data)
            {
                this.executionStages = executionStages;
            }

            /// <inheritdoc />
            public override void Start()
            {
                if (Data.AudioPlayer == null)
                {
                    Data.AudioPlayer = RuntimeConfigurator.Configuration.InstructionPlayer;
                }

                if ((Data.ExecutionStages & executionStages) > 0)
                {
                    if (Data.AudioData.HasAudioClip)
                    {
                        Data.AudioPlayer.clip = Data.AudioData.AudioClip;
                        Data.AudioPlayer.volume = Mathf.Clamp(Data.Volume, 0.0f, 1.0f);
                        Data.AudioPlayer.Play();
                    }
                    else
                    {
                        Debug.LogWarning("AudioData has no audio clip.");
                    }
                }
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                while ((Data.ExecutionStages & executionStages) > 0 && Data.AudioPlayer.isPlaying)
                {
                    yield return null;
                }
            }

            /// <inheritdoc />
            public override void End()
            {
                if ((Data.ExecutionStages & executionStages) > 0)
                {
                    Data.AudioPlayer.clip = null;
                }
            }

            /// <inheritdoc />
            public override void FastForward()
            {
                if ((Data.ExecutionStages & executionStages) > 0 && Data.AudioPlayer.isPlaying)
                {
                    Data.AudioPlayer.Stop();
                }
            }
        }

        [JsonConstructor, Preserve]
        protected PlayAudioBehavior() : this(null, BehaviorExecutionStages.None)
        {
        }

        public PlayAudioBehavior(IAudioData audioData, BehaviorExecutionStages executionStages, AudioSource audioPlayer = null, string name = "Play Audio")
        {
            Data.AudioData = audioData;
            Data.ExecutionStages = executionStages;
            Data.AudioPlayer = audioPlayer;
            Data.Name = name;
            Data.IsBlocking = true;
        }

        public PlayAudioBehavior(IAudioData audioData, BehaviorExecutionStages executionStages, bool isBlocking, AudioSource audioPlayer = null, string name = "Play Audio") : this(audioData, executionStages, audioPlayer, name)
        {
            Data.IsBlocking = isBlocking;
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new PlayAudioProcess(BehaviorExecutionStages.Activation, Data);
        }

        /// <inheritdoc />
        public override IStageProcess GetDeactivatingProcess()
        {
            return new PlayAudioProcess(BehaviorExecutionStages.Deactivation, Data);
        }
    }
}
