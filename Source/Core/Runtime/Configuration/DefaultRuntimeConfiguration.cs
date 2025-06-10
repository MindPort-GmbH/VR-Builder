// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Process runtime configuration which is used if no other was implemented.
    /// </summary>
    public class DefaultRuntimeConfiguration : BaseRuntimeConfiguration
    {
        private IProcessAudioPlayer processAudioPlayer;
        private ISceneObjectManager sceneObjectManager;

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
                UserSceneObject user = GameObject.FindObjectsByType<UserSceneObject>(FindObjectsSortMode.None).FirstOrDefault();

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
            get
            {
                return ProcessAudioPlayer.FallbackAudioSource;
            }
        }

        /// <inheritdoc />
        public override IProcessAudioPlayer ProcessAudioPlayer
        {
            get
            {
                if (processAudioPlayer == null)
                {
                    processAudioPlayer = new DefaultAudioPlayer();
                }

                return processAudioPlayer;
            }
        }

        /// <inheritdoc />
        public override ISceneObjectManager SceneObjectManager
        {
            get
            {
                if (sceneObjectManager == null)
                {
                    sceneObjectManager = new DefaultSceneObjectManager();
                }

                return sceneObjectManager;
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
