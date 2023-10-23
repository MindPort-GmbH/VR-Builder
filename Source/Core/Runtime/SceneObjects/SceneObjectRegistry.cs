// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Exceptions;
using VRBuilder.Unity;
using Object = UnityEngine.Object;

namespace VRBuilder.Core.SceneObjects
{
    /// <inheritdoc />
    public class SceneObjectRegistry : ISceneObjectRegistry
    {
        private readonly Dictionary<Guid, ISceneObject> registeredEntities = new Dictionary<Guid, ISceneObject>();

        /// <inheritdoc />
        public ISceneObject this[Guid guid]
        {
            get
            {
                return GetByGuid(guid);
            }
        }

        public SceneObjectRegistry()
        {
            RegisterAll();
        }

        /// <inheritdoc />
        public void RegisterAll()
        {
            foreach (ProcessSceneObject processObject in SceneUtils.GetActiveAndInactiveComponents<ProcessSceneObject>())
            {
                try
                {
                    Register(processObject);
                }
                catch (NameNotUniqueException)
                {
#if UNITY_EDITOR
                    string isPlayingText = Application.isPlaying ? "\n\nThe object will be restored after ending Play Mode." : "\n\nThe object will be deleted from the scene.";
                    if (EditorUtility.DisplayDialog("Scene Object Name Conflict", $"The game object {processObject.gameObject.name} cannot be registered because it has an already existing unique name: {processObject.Guid}. Do you want to delete it?{isPlayingText}", "Yes", "No"))
                    {
                        Object.DestroyImmediate(processObject.gameObject);
                    }
#else
                    Debug.LogErrorFormat("Registration of process object entity with name '{0}' failed. Name is not unique! Errors will ensue. Referenced game object: '{1}'.", processObject.Guid, processObject.GameObject.name);
#endif
                }
                catch (AlreadyRegisteredException)
                {
                }
            }
        }

        /// <inheritdoc />
        public ISceneObject this[string name]
        {
            get
            {
                return GetByName(name);
            }
        }

        /// <inheritdoc />
        public void Register(ISceneObject obj)
        {
            if (ContainsGuid(obj.Guid))
            {
                throw new AlreadyRegisteredException(obj);
            }

            registeredEntities.Add(obj.Guid, obj);
        }

        /// <inheritdoc />
        public bool Unregister(ISceneObject entity)
        {
            return registeredEntities.Remove(entity.Guid);
        }

        /// <inheritdoc />
        [Obsolete("Support for ISceneObject.UniqueName will be removed with VR-Builder 4")]
        public bool ContainsName(string guid)
        {
            return registeredEntities.Any(entity => entity.Value.Guid.ToString() == guid);
        }

        /// <inheritdoc />
        [Obsolete("Support for ISceneObject.UniqueName will be removed with VR-Builder 4. Guid string is used as name.")]
        public ISceneObject GetByName(string name)
        {
            System.Guid convertedGuid = new Guid(name);
            if (ContainsGuid(convertedGuid) == false)
            {
                throw new MissingEntityException(string.Format("Could not find scene entity '{0}'", name));
            }

            return registeredEntities.GetValueOrDefault(convertedGuid);
        }

        /// <inheritdoc />
        public bool ContainsGuid(Guid guid)
        {
            return registeredEntities.ContainsKey(guid);
        }

        /// <inheritdoc />
        public ISceneObject GetByGuid(Guid guid)
        {
            try
            {
                return registeredEntities[guid];
            }
            catch (KeyNotFoundException)
            {
                throw new MissingEntityException(string.Format("Could not find scene entity with identifier '{0}'", guid.ToString()));
            }
        }

        /// <inheritdoc />
        public IEnumerable<ISceneObject> GetByTag(Guid tag)
        {
            return registeredEntities.Values.Where(entity => entity as ITagContainer != null && ((ITagContainer)entity).HasTag(tag));
        }

        /// <inheritdoc />
        public IEnumerable<T> GetPropertyByTag<T>(Guid tag)
        {
            return GetByTag(tag)
                .Where(sceneObject => sceneObject.Properties.Any(property => property is T))
                .Select(sceneObject => sceneObject.Properties.First(property => property is T))
                .Cast<T>();
        }
    }
}
