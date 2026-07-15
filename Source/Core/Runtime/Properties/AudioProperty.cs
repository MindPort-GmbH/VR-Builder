// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using Core.Runtime.Utils;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Runtime.Utils;
using VRBuilder.Core.Utils.Audio;

namespace VRBuilder.Core.Properties
{
    public class AudioProperty : ProcessSceneObjectProperty, IAudioPlayer
    {
        [SerializeField]
        private AudioSource audioSource;

        private IAudioData fallbackAudioSource;

        /// <inheritdoc />
        public IAudioData FallbackAudioSource => fallbackAudioSource;

        /// <inheritdoc />
        public bool IsPlaying => audioSource.isPlaying || AudioListener.pause;

        /// <inheritdoc />
        public bool IsMute => audioSource.mute;

        private void Start()
        {
            if (!audioSource)
            {
                audioSource = RuntimeConfigurator.Configuration.User.Head.gameObject.GetComponent<AudioSource>();
            }
        }
        
        /// <inheritdoc />
        public void PlayAudio(IAudioData audioData, float volume = 1, float pitch = 1)
        {
            audioSource.clip = audioData.AudioClip.ToUnity();
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.Play();
        }

        /// <inheritdoc />
        public void ResetAudio()
        {
            audioSource.volume = 0; //TODO we shouldn't set volume if it is player head bound
            audioSource.clip = null;
            // audioPlayer = Data.AudioPlayer ? new DefaultAudioPlayer(Data.AudioPlayer) : RuntimeConfigurator.Configuration.ProcessAudioPlayer;
            // audioPlayer.Reset();
        }

        public void StopAudio()
        {
            audioSource.volume = 0;
            audioSource.Stop();
            audioSource.clip = null;
        }
    }
}