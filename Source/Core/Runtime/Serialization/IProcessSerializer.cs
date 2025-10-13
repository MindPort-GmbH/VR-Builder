// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using VRBuilder.Core.IO;

namespace VRBuilder.Core.Serialization
{
    /// <summary>
    /// A serializer which can de/serialize processes and steps to a certain format.
    /// </summary>
    public interface IProcessSerializer
    {
        /// <summary>
        /// Display name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// File format used for this serializer. For example, 'json'.
        /// </summary>
        string FileFormat { get; }

        /// <summary>
        /// Serializes a given entity into a byte array.
        /// </summary>
        byte[] EntityToByteArray(IEntity entity);

        /// <summary>
        /// Deserializes a given entity to a usable object.
        /// </summary>
        IEntity EntityFromByteArray(byte[] data);

        /// <summary>
        /// Serializes a given process into a byte array.
        /// </summary>
        byte[] ProcessToByteArray(IProcess target);

        /// <summary>
        /// Deserializes a given process to a usable object.
        /// </summary>
        IProcess ProcessFromByteArray(byte[] data);

        /// <summary>
        /// Serializes a given chapter into a byte array.
        /// </summary>
        byte[] ChapterToByteArray(IChapter chapter);

        /// <summary>
        /// Deserializes a given chapter to a usable object.
        /// </summary>
        IChapter ChapterFromByteArray(byte[] data);

        /// <summary>
        /// Serializes a given step into a byte array. The implementation should trim target steps of the step.
        /// </summary>
        byte[] StepToByteArray(IStep step);

        /// <summary>
        /// Deserializes a given step to a usable object.
        /// </summary>
        IStep StepFromByteArray(byte[] data);

        /// <summary>
        /// Serializes a process asset manifest into a byte array.
        /// </summary>
        byte[] ManifestToByteArray(IProcessAssetManifest manifest);

        /// <summary>
        /// Deserializes a process asset manifest to a usable object.
        /// </summary>
        IProcessAssetManifest ManifestFromByteArray(byte[] data);
    }
}
