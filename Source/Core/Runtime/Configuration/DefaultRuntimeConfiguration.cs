// Copyright (c) 2013-2019 Innoactive GmbH
// Modifications copyright (c) 2021-2026 MindPort GmbH
// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Runtime.Utils;
using UnityEngine;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using Object = UnityEngine.Object;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Process runtime configuration which is used if no other was implemented.
    /// </summary>
    public class DefaultRuntimeConfiguration : BaseRuntimeConfiguration
    {
        private IAudioPlayer audioPlayer;

        /// <summary>
        /// Default mode which white lists everything.
        /// </summary>
        public static readonly IMode DefaultMode = new Mode("Default", new WhitelistTypeRule<IOptional>());

        public DefaultRuntimeConfiguration()
        {
            Modes = new BaseModeHandler(new List<IMode> { DefaultMode });
        }

        /// <inheritdoc />
        [Obsolete("Use User property instead.")]
        public override UserSceneObject LocalUser
        {
            get
            {
                UserSceneObject user = User as UserSceneObject;

                if (user == null)
                {
                    throw new Exception("Could not find a UserSceneObject in the scene.");
                }

                return user;
            }
        }

        /// <inheritdoc />
        public override IXRRigTransform User
        {
            get
            {
                UserSceneObject user = Object.FindObjectsByType<UserSceneObject>(FindObjectsSortMode.None).FirstOrDefault();

                if (user == null)
                {
                    throw new Exception("Could not find a user rig in the scene.");
                }

                return user;
            }
        }

        /// <inheritdoc />
        public override AudioSource InstructionPlayer
        {
            get { return AudioPlayer.FallbackAudioSource.ToUnity((AudioPlayer as Component)?.gameObject); }
        }

        /// <inheritdoc />
        public override IAudioPlayer AudioPlayer
        {
            get
            {
                if (audioPlayer == null)
                {
                    audioPlayer = User.Head.GetComponentInChildren<AudioProperty>();
                }

                return audioPlayer;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<IXRRigTransform> UserTransforms
        {
            get
            {
                if (User != null)
                {
                    return new List<IXRRigTransform>() { User };
                }
                else
                {
                    return new List<IXRRigTransform>();
                }
            }
        }
    }
}