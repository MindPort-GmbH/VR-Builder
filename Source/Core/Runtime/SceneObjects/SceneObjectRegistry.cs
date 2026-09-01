// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Implementation of <see cref="ISceneObjectRegistry"/> that handles <see cref="ISceneObject"/>s with one
    /// or more GUID associated to them. The GUIDs don't have to be unique and can represent a group of objects.
    /// </summary>
    public class SceneObjectRegistry : ISceneObjectRegistry
    {
        protected readonly Dictionary<Guid, List<ISceneObject>> registeredObjects = new Dictionary<Guid, List<ISceneObject>>();

        /// <inheritdoc/>
        public event Action Changed;

        private bool suppressChangeNotification;

        private ISceneObjectRegistryConfiguration configuration;

        /// <summary>
        /// Flag to track if there are any dirty scene objects that need registry refresh.
        /// </summary>
        /// <remarks>
        /// Tracks state swishes of prefabs between edit mode and the main scene.
        /// </remarks>
        private bool hasDirtySceneObjects = false;

        public IEnumerable<Guid> RegisteredGuids => registeredObjects.Keys;

        public ISceneObject this[Guid guid] => GetByGuid(guid);

        public ISceneObjectGroups SceneObjectGroups => VRBuilder.Core.Settings.SceneObjectGroups.Instance;

        /// <inheritdoc/>
        public bool ContainsGuid(Guid guid)
        {
            return registeredObjects.ContainsKey(guid);
        }

        public ISceneObject GetByGuid(Guid guid)
        {
            return registeredObjects.TryGetValue(guid, out var o) ? o.FirstOrDefault() : null;
        }

        /// <inheritdoc/>
        public IEnumerable<ISceneObject> GetObjects(Guid guid)
        {
            if (registeredObjects.ContainsKey(guid))
            {
                if (registeredObjects[guid].Any(obj => obj.Equals(null)))
                {
                    string key = SceneObjectGroups.GetLabel(guid);

                    if (string.IsNullOrEmpty(key))
                    {
                        key = guid.ToString();
                    }

                    // This happens if we remove a SceneObject from component where no registry is available
                    // e.g.: From a prefab in prefab edit mode
                    ForwardingLogger.LogError($"Null objects found in scene object registry for with Guid {key}: " +
                                              $"{registeredObjects[guid].Count(obj => obj.Equals(null))} object. " +
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
        public IEnumerable<T> GetAllProperties<T>() where T : ISceneObjectProperty
        {
            return registeredObjects.Keys
                .SelectMany(GetProperties<T>)
                .Distinct();
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

                ForwardingLogger.LogWarning($"Found a duplicate in the registry for {obj}. A new object ID has been assigned.");

                configuration?.EditorPrefabHandler.OnDuplicateGuidDetected(obj);
            }

            foreach (Guid guid in GetAllGuids(obj))
            {
                RegisterGuid(obj, guid);
            }

            obj.GuidAdded += OnGuidAdded;
            obj.GuidRemoved += OnGuidRemoved;

            RefreshIfDirty();

            if (suppressChangeNotification == false)
            {
                NotifyChanged();
            }
        }

        private bool HasDuplicateGuid(ISceneObject obj)
        {
            if (!ContainsGuid(obj.Guid)) return false;
            var identity = configuration?.SceneObjectIdentity;
            if (identity == null) return true;

            var objId = identity.GetIdentity(obj);
            return GetObjects(obj.Guid).Any(existing => identity.GetIdentity(existing) != objId);
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
            NotifyChanged();
        }

        private void OnGuidRemoved(object sender, GuidContainerEventArgs args)
        {
            if (registeredObjects.ContainsKey(args.Guid))
            {
                registeredObjects[args.Guid].Remove((ISceneObject)sender);


                if (!registeredObjects[args.Guid].Any())
                {
                    registeredObjects.Remove(args.Guid);
                }
            }

            NotifyChanged();
        }

        /// <inheritdoc/>
        public void RegisterAll()
        {
            suppressChangeNotification = true;
            try
            {
                var finder = configuration?.SceneObjectFinder;
                if (finder == null) return;
                foreach (var processObject in finder.FindAllSceneObjects<ISceneObject>())
                    Register(processObject);
            }
            finally
            {
                suppressChangeNotification = false;
            }

            NotifyChanged();
        }

        /// <inheritdoc/>
        public void Refresh()
        {
            RemoveAllObjectsNotInScene();
            RegisterAll();
            ForwardingLogger.Log("Refreshed SceneObjectRegistry");
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

                    if (!registeredObjects[guid].Any())
                    {
                        registeredObjects.Remove(guid);
                    }
                }
            }

            RefreshIfDirty();

            NotifyChanged();
            return wasUnregistered;
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }

        private IEnumerable<Guid> GetAllGuids(ISceneObject obj)
        {
            return new HashSet<Guid> { obj.Guid }.Concat(obj.Guids);
        }

        public void SetConfiguration(ISceneObjectRegistryConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Initialize()
        {
            RegisterAll();
        }

        /// <summary>
        /// Marks a scene object's prefab as dirty, indicating that the registry needs refreshing.
        /// </summary>
        /// <param name="sceneObject">The scene object to mark as dirty (currently not used).</param>
        /// <remarks>
        /// This currently triggers a full refresh of the SceneObjectRegistry
        /// </remarks>
        public void MarkSceneObjectDirty(ISceneObject sceneObject)
        {
            hasDirtySceneObjects = true;
        }

        /// <summary>
        /// Refreshes the scene object registry if there are dirty scene objects that need to be updated.
        /// </summary>
        public void RefreshIfDirty()
        {
            if (hasDirtySceneObjects)
            {
                hasDirtySceneObjects = false;
                Refresh();
            }
        }
    }
}
