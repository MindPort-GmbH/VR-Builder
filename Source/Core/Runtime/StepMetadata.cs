// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace VRBuilder.Core
{
    /// <summary>
    /// Implementation of <see cref="IMetadata"/> adapted for <see cref="IStep"/> data.
    /// </summary>
    public class StepMetadata : IMetadata
    {
        /// <summary>
        /// Graphical position of current <see cref="IStep"/> on the 'Workflow' window.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Graphical representation of current <see cref="IStep"/> on the 'Workflow' window.
        /// </summary>
        public string StepType { get; set; }

        /// <summary>
        /// Unique identifier for step.
        /// </summary>
        [DataMember]
        public Guid Guid { get; set; }
    }
}
