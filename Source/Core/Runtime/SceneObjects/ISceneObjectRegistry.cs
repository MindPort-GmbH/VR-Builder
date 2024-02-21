// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

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
        /// Returns if the name is registered in the registry.
        /// </summary>
        [Obsolete("Still supports passing a guid as a string. Please consider using ContainsGuid instead.")]
        bool ContainsName(string name);

        /// <summary>
        /// Returns the IProcessSceneEntity belonging to the given Guid.
        /// If there is no fitting Entity found a MissingEntityException will be thrown.
        /// </summary>
        [Obsolete("Use GetByTag instead.")]
        ISceneObject GetByGuid(Guid guid);

        /// <summary>
        /// Returns the IProcessSceneEntity belonging to the given unique name.
        /// If there is no fitting Entity found a MissingEntityException will be thrown.
        /// </summary>
        [Obsolete("Use GetByTag instead.")]
        ISceneObject GetByName(string name);

        /// <summary>
        /// Returns all registered scene objects which have the provided guid assigned to them.
        /// </summary>
        IEnumerable<ISceneObject> GetByTag(Guid tag);

        /// <summary>
        /// Returns all registered scene objects with the provided guid and at least one valid property of the specified type.
        /// </summary>
        IEnumerable<T> GetPropertyByTag<T>(Guid tag) where T : ISceneObjectProperty;

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
        /// Shortcut for GetByName(string name) method.
        /// </summary>
        [Obsolete("Use GetByTag instead")]
        ISceneObject this[string name] { get; }

        /// <summary>
        /// Shortcut for GetByGuid(Guid guid) method.
        /// </summary>
        [Obsolete("Use GetByTag instead")]
        ISceneObject this[Guid guid] { get; }

        /// <summary>
        /// Registers all SceneObject in scene, independent of their state.
        /// </summary>
        void RegisterAll();
    }
}
