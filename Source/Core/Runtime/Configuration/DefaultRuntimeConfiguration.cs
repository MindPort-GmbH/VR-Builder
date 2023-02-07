// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using UnityEngine;
using System;
using System.Collections.Generic;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.IO;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Properties;
using VRBuilder.Core.Serialization;
using VRBuilder.Core.Serialization.NewtonsoftJson;
using System.Linq;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Process runtime configuration which is used if no other was implemented.
    /// </summary>
    public class DefaultRuntimeConfiguration : BaseRuntimeConfiguration
    {
        private AudioSource instructionPlayer;

        /// <summary>
        /// Default mode which white lists everything.
        /// </summary>
        public static readonly IMode DefaultMode = new Mode("Default", new WhitelistTypeRule<IOptional>());

        public DefaultRuntimeConfiguration()
        {
            Modes = new BaseModeHandler(new List<IMode> {DefaultMode});
        }

        /// <inheritdoc />
        public override ProcessSceneObject User
        {
            get
            {
                return Users.FirstOrDefault();
            }
        }

        /// <inheritdoc />
        public override AudioSource InstructionPlayer
        {
            get
            {
                return Users.FirstOrDefault() == null ? null : Users.FirstOrDefault().ProcessAudioSource;
            }
        }

        public override IEnumerable<UserSceneObject> Users => GameObject.FindObjectsOfType<UserSceneObject>();
    }
}
