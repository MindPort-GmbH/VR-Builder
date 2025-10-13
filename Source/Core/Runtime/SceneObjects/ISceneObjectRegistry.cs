// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.SceneObjects
{
    public interface ISceneObjectRegistry
    {
        /// <summary>
        /// Returns if the Guid is registered in the registry.
        /// </summary>
        bool ContainsGuid(Guid guid);

        /// <summary>
        /// Returns all registered scene objects which have the provided guid assigned to them.
        /// </summary>
        IEnumerable<ISceneObject> GetObjects(Guid guid);

        /// <summary>
        /// Returns all registered scene objects with the provided guid and at least one valid property of the specified type.
        /// </summary>
        IEnumerable<T> GetProperties<T>(Guid guid) where T : ISceneObjectProperty;

        /// <summary>
        /// Registers an SceneObject in the registry. If there is an SceneObject with the same name
        /// already registered, an NameNotUniqueException will be thrown. Also if the Guid
        /// is already known an SceneObjectAlreadyRegisteredException will be thrown.
        /// </summary>
        void Register(ISceneObject obj);

        /// <summary>
        /// Removes the SceneObject completely from the Registry.
        /// </summary>
        bool Unregister(ISceneObject obj);

        /// <summary>
        /// Registers all SceneObject in scene, independent of their state.
        /// </summary>
        void RegisterAll();

        /// <summary>
        /// Updates the registry by removing all <see cref="ISceneObject"/> which are not in the scene anymore and adding new ones.
        /// </summary>
        void Refresh();
    }
}
