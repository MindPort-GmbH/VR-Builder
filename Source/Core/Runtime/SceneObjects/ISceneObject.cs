// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System;
using System.Collections.Generic;
using UnityEngine;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.SceneObjects
{
    [Obsolete("These args belong to a unused event and will be removed in the next major release.")]
    public class SceneObjectNameChanged : EventArgs
    {
        public readonly string NewName;
        public readonly string PreviousName;

        public SceneObjectNameChanged(string newName, string previousName)
        {
            NewName = newName;
            PreviousName = previousName;
        }
    }

    /// <summary>
    /// Gives the possibility to easily identify targets for Conditions, Behaviors and so on.
    /// </summary>
    public interface ISceneObject : ILockable, ITagContainer
    {
        [Obsolete("This event is no longer used and will be removed in the next major release.")]
        event EventHandler<SceneObjectNameChanged> UniqueNameChanged;

        /// <summary>
        /// Unique Guid for each entity, which is required
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// Unique name which is not required
        /// </summary>
        [Obsolete("Use Guid instead.")]
        string UniqueName { get; }

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

        /// <summary>
        /// Validates properties on the scene object.
        /// </summary>        
        void ValidateProperties(IEnumerable<Type> properties);

        /// <summary>
        /// Returns a property of the specified type.
        /// </summary>
        T GetProperty<T>() where T : ISceneObjectProperty;

        /// <summary>
        /// Attempts to change the scene object's unique name to the specified name.
        /// </summary>
        [Obsolete("Use ChangeUniqueId instead.")]
        void ChangeUniqueName(string newName);

        /// <summary>
        /// Changes the scene object's unique id to the specified name.
        /// </summary>
        void ChangeUniqueId(Guid newGuid);

        /// TODO do we need this its only used by a Test ChangingNameUpdatesRegistration
        /// <summary>
        /// Gives the object a new specified unique id.
        /// </summary>
        void SetUniqueId(Guid guid);
    }
}
