using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.Properties;
using VRBuilder.Core.Settings;
using VRBuilder.Unity;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Implementation of <see cref="ISceneObjectRegistry"/> that handles <see cref="ISceneObject"/>s with one
    /// or more GUID associated to them. The GUIDs don't have to be unique and can represent a group of objects.
    /// </summary>
    public class GuidBasedSceneObjectRegistry : ISceneObjectRegistry
    {
        protected readonly Dictionary<Guid, List<ISceneObject>> registeredObjects = new Dictionary<Guid, List<ISceneObject>>();

        public IEnumerable<Guid> RegisteredGuids => registeredObjects.Keys;

        /// <inheritdoc/>
        public ISceneObject this[string name] => GetByName(name);

        /// <inheritdoc/>
        public ISceneObject this[Guid guid] => GetByGuid(guid);

        public GuidBasedSceneObjectRegistry()
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
                    string key = SceneObjectGroups.Instance.GetLabel(guid);

                    if (string.IsNullOrEmpty(key))
                    {
                        key = guid.ToString();
                    }

                    // This happens if we remove a SceneObject from component where no registry is available
                    // e.g.: From a prefab in prefab edit mode
                    Debug.LogError($"Null objects found in scene object registry for with Guid {key}: " +
                        $"{registeredObjects[guid].Where(obj => obj.Equals(null)).Count()} object. " +
                        $"Most likely you removed a process scene object from a prefab in prefab edit mode. " +
                        $"Removing the reference it from the registry.");

                    registeredObjects.Remove(guid);
                    return new List<ISceneObject>();
                }

                return registeredObjects[guid].Where(obj => obj.Equals(null) == false);
            }
            else
            {
                return new List<ISceneObject>();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetProperties<T>(Guid guid) where T : ISceneObjectProperty
        {
            return GetObjects(guid)
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

            if (HasDuplicateGuid(obj))
            {
                obj.SetObjectId(Guid.NewGuid());

                Debug.LogWarning($"Found a duplicate in the registry for {obj.GameObject.name}. A new object ID has been assigned.");

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

            foreach (Guid guid in GetAllGuids(obj))
            {
                RegisterGuid(obj, guid);
            }

            obj.GuidAdded += OnGuidAdded;
            obj.GuidRemoved += OnGuidRemoved;
        }

        private bool HasDuplicateGuid(ISceneObject obj)
        {
            if (ContainsGuid(obj.Guid) == false)
            {
                return false;
            }

            IEnumerable<ISceneObject> sceneObjects = GetObjects(obj.Guid);
            return sceneObjects.Select(so => so.GameObject.GetInstanceID()).Contains(obj.GameObject.GetInstanceID()) == false;
        }

        private void RegisterGuid(ISceneObject sceneObject, Guid guid)
        {
            if (registeredObjects.ContainsKey(guid))
            {
                if (registeredObjects[guid].Contains(sceneObject) == false)
                {
                    registeredObjects[guid].Add(sceneObject);
                }
            }
            else
            {
                registeredObjects.Add(guid, new List<ISceneObject>() { sceneObject });
            }
        }

        private void OnGuidAdded(object sender, GuidContainerEventArgs args)
        {
            RegisterGuid((ISceneObject)sender, args.Guid);
        }

        private void OnGuidRemoved(object sender, GuidContainerEventArgs args)
        {
            if (registeredObjects.ContainsKey(args.Guid))
            {
                registeredObjects[args.Guid].Remove((ISceneObject)sender);


                if (registeredObjects[args.Guid].Count() == 0)
                {
                    registeredObjects.Remove(args.Guid);
                }
            }
        }

        /// <inheritdoc/>
        public void RegisterAll()
        {
            foreach (ProcessSceneObject processObject in SceneUtils.GetActiveAndInactiveComponents<ProcessSceneObject>())
            {
                Register(processObject);
            }
        }

        /// <inheritdoc/>
        public void Refresh()
        {
            RemoveAllObjectsNotInScene();
            RegisterAll();
            Debug.Log("Refreshed SceneObjectRegistry");
        }

        /// <summary>
        /// Removes all objects that are no longer in the scene from the registeredObjects dictionary.
        /// </summary>
        private void RemoveAllObjectsNotInScene()
        {
            foreach (var entry in registeredObjects)
            {
                entry.Value.RemoveAll(obj => obj == null);
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
                throw new NullReferenceException("Attempted to unregister a null object.");
            }

            obj.GuidAdded -= OnGuidAdded;
            obj.GuidRemoved -= OnGuidRemoved;

            foreach (Guid guid in GetAllGuids(obj))
            {
                if (registeredObjects.ContainsKey(guid))
                {
                    wasUnregistered &= registeredObjects[guid].Remove(obj);

                    if (registeredObjects[guid].Count() == 0)
                    {
                        registeredObjects.Remove(guid);
                    }
                }
            }

            return wasUnregistered;
        }

        private IEnumerable<Guid> GetAllGuids(ISceneObject obj)
        {
            return new List<Guid>() { obj.Guid }.Concat(obj.Guids);
        }
    }
}
