// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System;
using System.Collections.Generic;
using UnityEngine;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Gives the possibility to easily identify targets for Conditions, Behaviors and so on.
    /// </summary>
    public interface ISceneObject : ILockable
    {
        /// <summary>
        /// Unique Guid for each entity, which is required
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// Unique name which is not required
        /// </summary>
        [Obsolete("Support for ISceneObject.UniqueName will be removed with VR-Builder 4.  Guid string is returned as name.")]
        string UniqueName { get => Guid.ToString(); }

        /// <summary>
        /// Target GameObject, used for applying stuff.
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// Properties on the scene object.
        /// </summary>
        ICollection<ISceneObjectProperty> Properties { get; }

        /// <summary>
        /// True if the scene object has a property of the specified type.
        /// </summary>
        bool CheckHasProperty<T>() where T : ISceneObjectProperty;

        /// <summary>
        /// True if the scene object has a property of the specified type.
        /// </summary>
        bool CheckHasProperty(Type type);

        void ValidateProperties(IEnumerable<Type> properties);

        /// <summary>
        /// Returns a property of the specified type.
        /// </summary>
        T GetProperty<T>() where T : ISceneObjectProperty;
    }
}
