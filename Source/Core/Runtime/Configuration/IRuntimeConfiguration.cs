// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System;
using UnityEngine;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Serialization;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// An interface for process runtime configurations. Implement it to create your own.
    /// </summary>
    [Obsolete("To be more flexible with development we switched to an abstract class as configuration base, consider using BaseRuntimeConfiguration.")]
    public interface IRuntimeConfiguration
    {
        /// <summary>
        /// SceneObjectRegistry gathers all created ProcessSceneEntities.
        /// </summary>
        ISceneObjectRegistry SceneObjectRegistry { get; }

        /// <summary>
        /// Defines the serializer which should be used to serialize processes.
        /// </summary>
        IProcessSerializer Serializer { get; set; }

        /// <summary>
        /// Returns the mode handler for the process.
        /// </summary>
        IModeHandler Modes { get; }

        /// <summary>
        /// User scene object.
        /// </summary>
        ProcessSceneObject User { get; }

        /// <summary>
        /// Default audio source to play audio from.
        /// </summary>
        AudioSource InstructionPlayer { get; }

        /// <summary>
        /// Synchronously returns the deserialized process from given path.
        /// </summary>
        IProcess LoadProcess(string path);
    }
}
