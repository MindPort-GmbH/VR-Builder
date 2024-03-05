using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.Exceptions;
using VRBuilder.Core.Properties;
using VRBuilder.Core.Settings;
using VRBuilder.Unity;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Implementation of <see cref="ISceneObjectRegistry"/> that handles <see cref="ISceneObject"/>s with one
    /// or more GUID associated to them. The GUIDs don't have to be unique and can represent a group of objects.
    /// </summary>
    public class SceneObjectRegistryV2 : ISceneObjectRegistry
    {
        protected readonly Dictionary<Guid, List<ISceneObject>> registeredObjects = new Dictionary<Guid, List<ISceneObject>>();

        /// <inheritdoc/>
        public IEnumerable<Guid> RegisteredGuids => registeredObjects.Keys;

        /// <inheritdoc/>
        public ISceneObject this[string name] => GetByName(name);

        /// <inheritdoc/>
        public ISceneObject this[Guid guid] => GetByGuid(guid);

        public SceneObjectRegistryV2()
        {
            RegisterAll();
        }

        /// <inheritdoc/>
        public bool ContainsGuid(Guid guid)
        {
            return registeredObjects.ContainsKey(guid);
        }

        /// <inheritdoc/>
        public bool ContainsName(string guidString)
        {
            Guid guid;

            if (Guid.TryParse(guidString, out guid))
            {
                return ContainsGuid(guid);
            }

            return false;
        }

        /// <inheritdoc/>
        public ISceneObject GetByGuid(Guid guid)
        {
            if (registeredObjects.ContainsKey(guid))
            {
                return registeredObjects[guid].FirstOrDefault();
            }

            return null;
        }

        /// <inheritdoc/>
        public ISceneObject GetByName(string name)
        {
            Guid guid;

            if (Guid.TryParse(name, out guid))
            {
                return GetByGuid(guid);
            }

            return null;
        }

        /// <inheritdoc/>
        public IEnumerable<ISceneObject> GetObjects(Guid guid)
        {
            if (registeredObjects.ContainsKey(guid))
            {
                if (registeredObjects[guid].Any(obj => obj.Equals(null)))
                {
                    string key = SceneObjectTags.Instance.GetLabel(guid);

                    if (string.IsNullOrEmpty(key))
                    {
                        key = guid.ToString();
                    }

                    Debug.LogError($"Null objects found in scene object registry for key {key}: {registeredObjects[guid].Where(obj => obj.Equals(null)).Count()} object.");
                }

                return registeredObjects[guid].Where(obj => obj.Equals(null) == false);
            }
            else
            {
                return new List<ISceneObject>();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetProperties<T>(Guid tag) where T : ISceneObjectProperty
        {
            return GetObjects(tag)
                .Where(so => so.CheckHasProperty<T>())
                .Select(so => so.GetProperty<T>());
        }

        /// <inheritdoc/>
        public void Register(ISceneObject obj)
        {
            if (obj == null)
            {
                throw new NullReferenceException("Attempted to register a null object.");
            }

            if (HasDuplicateUniqueTag(obj))
            {
                obj.SetUniqueId(Guid.NewGuid());

                Debug.LogWarning($"Found a duplicate in the registry for {obj.GameObject.name}. A new unique id has been assigned.");

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(obj.GameObject);

                if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(obj.GameObject))
                {
                    var prefabInstance = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(obj.GameObject);
                    if (prefabInstance != null)
                    {
                        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(prefabInstance);
                    }
                }
#endif
            }

            foreach (Guid tag in GetAllGuids(obj))
            {
                RegisterTag(obj, tag);
            }

            obj.TagAdded += OnTagAdded;
            obj.TagRemoved += OnTagRemoved;
        }

        private bool HasDuplicateUniqueTag(ISceneObject obj)
        {
            if (ContainsGuid(obj.Guid) == false)
            {
                return false;
            }

            IEnumerable<ISceneObject> sceneObjects = GetObjects(obj.Guid);
            return sceneObjects.Select(so => so.GameObject.GetInstanceID()).Contains(obj.GameObject.GetInstanceID()) == false;
        }

        private void RegisterTag(ISceneObject sceneObject, Guid guid)
        {
            if (registeredObjects.ContainsKey(guid))
            {
                if (registeredObjects[guid].Contains(sceneObject) == false)
                {
                    registeredObjects[guid].Add(sceneObject);
                }
                else
                {
                    // throw new AlreadyRegisteredException((ISceneObject)tagContainer);
                }
            }
            else
            {
                registeredObjects.Add(guid, new List<ISceneObject>() { sceneObject });
            }
        }

        private void OnTagAdded(object sender, TaggableObjectEventArgs args)
        {
            RegisterTag((ISceneObject)sender, args.Tag);
        }

        private void OnTagRemoved(object sender, TaggableObjectEventArgs args)
        {
            if (registeredObjects.ContainsKey(args.Tag))
            {
                registeredObjects[args.Tag].Remove((ISceneObject)sender);


                if (registeredObjects[args.Tag].Count() == 0)
                {
                    registeredObjects.Remove(args.Tag);
                }
            }
        }

        /// <inheritdoc/>
        public void RegisterAll()
        {
            foreach (ProcessSceneObject processObject in SceneUtils.GetActiveAndInactiveComponents<ProcessSceneObject>())
            {
                try
                {
                    Register(processObject);
                }
                catch (AlreadyRegisteredException)
                {
                }
            }
        }

        /// <summary>
        /// Clears the registry and registers all object in the scene again.
        /// </summary>
        public void DebugRebuild()
        {
            registeredObjects.Clear();
            RegisterAll();
        }

        /// <inheritdoc/>
        public bool Unregister(ISceneObject obj)
        {
            bool wasUnregistered = true;

            if (obj == null)
            {
                // TODO exception
            }

            obj.TagAdded -= OnTagAdded;
            obj.TagRemoved -= OnTagRemoved;

            foreach (Guid tag in GetAllGuids(obj))
            {
                if (registeredObjects.ContainsKey(tag))
                {
                    wasUnregistered &= registeredObjects[tag].Remove(obj);

                    if (registeredObjects[tag].Count() == 0)
                    {
                        registeredObjects.Remove(tag);
                    }
                }
            }

            return wasUnregistered;
        }

        private IEnumerable<Guid> GetAllGuids(ISceneObject obj)
        {
            return new List<Guid>() { obj.Guid }.Concat(obj.Tags);
        }

        /// <inheritdoc/>
        [Obsolete("Use GetObjects instead.")]
        public IEnumerable<ISceneObject> GetByTag(Guid tag)
        {
            return GetObjects(tag);
        }

        /// <inheritdoc/>
        [Obsolete("Use GetProperties instead.")]
        public IEnumerable<T> GetPropertyByTag<T>(Guid tag) where T : ISceneObjectProperty
        {
            return GetProperties<T>(tag);
        }
    }
}
