// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using UnityEngine;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Arguments for UniqueIdChanged event.
    /// </summary>
    public class UniqueIdChangedEventArgs : EventArgs
    {
        public readonly Guid NewId;
        public readonly Guid PreviousId;

        public UniqueIdChangedEventArgs(Guid previousId, Guid newId)
        {
            NewId = newId;
            PreviousId = previousId;
        }
    }

    /// <summary>
    /// Gives the possibility to easily identify targets for Conditions, Behaviors and so on.
    /// </summary>
    public interface ISceneObject : ILockable, IGuidContainer
    {
        /// <summary>
        /// Called when the object's object id has been changed.
        /// </summary>
        event EventHandler<UniqueIdChangedEventArgs> ObjectIdChanged;

        /// <summary>
        /// Unique Guid for each entity, which is required
        /// </summary>
        Guid Guid { get; }

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
        /// Gives the object a new specified unique ID.
        /// </summary>
        void SetObjectId(Guid guid);
    }
}
