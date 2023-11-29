using System;
using System.Collections.Generic;
using System.Linq;
using VRBuilder.Core.Exceptions;
using VRBuilder.Unity;

namespace VRBuilder.Core.SceneObjects
{
    public class NonUniqueSceneObjectRegistry : ISceneObjectRegistry
    {
        protected readonly Dictionary<Guid, List<ITagContainer>> registeredObjects = new Dictionary<Guid, List<ITagContainer>>();

        public IEnumerable<Guid> RegisteredGuids => registeredObjects.Keys;

        public ISceneObject this[string name] => GetByName(name);

        public ISceneObject this[Guid guid] => GetByGuid(guid);

        public NonUniqueSceneObjectRegistry()
        {
            RegisterAll();
        }

        public bool ContainsGuid(Guid guid)
        {
            return registeredObjects.ContainsKey(guid);
        }

        public bool ContainsName(string guid)
        {
            return ContainsGuid(Guid.Parse(guid));
        }

        public ISceneObject GetByGuid(Guid guid)
        {
            return registeredObjects[guid].FirstOrDefault() as ISceneObject;
        }

        public ISceneObject GetByName(string name)
        {
            return GetByGuid(Guid.Parse(name));
        }

        public IEnumerable<ISceneObject> GetByTag(Guid tag)
        {
            if (registeredObjects.ContainsKey(tag))
            {
                return registeredObjects[tag].Cast<ISceneObject>();
            }
            else
            {
                return new List<ISceneObject>();
            }
        }

        public IEnumerable<T> GetPropertyByTag<T>(Guid tag)
        {
            return GetByTag(tag).Select(sceneObject => sceneObject.GameObject.GetComponent<T>()).Where(property => property != null);
        }

        public void Register(ISceneObject obj)
        {
            ITagContainer tagContainer = obj as ITagContainer;

            if (tagContainer == null)
            {
                // TODO exception
            }

            foreach (Guid tag in tagContainer.Tags)
            {
                RegisterTag(tagContainer, tag);
            }

            tagContainer.TagAdded += OnTagAdded;
            tagContainer.TagRemoved += OnTagRemoved;
        }

        private void RegisterTag(ITagContainer tagContainer, Guid guid)
        {
            if (registeredObjects.ContainsKey(guid))
            {
                if (registeredObjects[guid].Contains(tagContainer) == false)
                {
                    registeredObjects[guid].Add(tagContainer);
                }
                else
                {
                    // throw new AlreadyRegisteredException((ISceneObject)tagContainer);
                }
            }
            else
            {
                registeredObjects.Add(guid, new List<ITagContainer>() { tagContainer });
            }
        }

        private void OnTagAdded(object sender, TaggableObjectEventArgs args)
        {
            RegisterTag((ITagContainer)sender, args.Tag);
        }

        private void OnTagRemoved(object sender, TaggableObjectEventArgs args)
        {
            if (registeredObjects.ContainsKey(args.Tag))
            {
                registeredObjects[args.Tag].Remove((ITagContainer)sender);


                if (registeredObjects[args.Tag].Count() == 0)
                {
                    registeredObjects.Remove(args.Tag);
                }
            }
        }

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

        public bool TryGetGuid(Guid guid, out ISceneObject entity)
        {
            if (registeredObjects.ContainsKey(guid))
            {
                entity = registeredObjects[guid].First() as ISceneObject;
            }
            else
            {
                entity = null;
            }

            return entity != null;
        }

        public bool Unregister(ISceneObject obj)
        {
            bool wasUnregistered = true;

            ITagContainer tagContainer = obj as ITagContainer;

            if (tagContainer == null)
            {
                // TODO exception
            }

            foreach (Guid tag in tagContainer.Tags)
            {
                if (registeredObjects.ContainsKey(tag))
                {
                    wasUnregistered &= registeredObjects[tag].Remove(tagContainer);

                    if (registeredObjects[tag].Count() == 0)
                    {
                        registeredObjects.Remove(tag);
                    }
                }
            }

            return wasUnregistered;
        }
    }
}
