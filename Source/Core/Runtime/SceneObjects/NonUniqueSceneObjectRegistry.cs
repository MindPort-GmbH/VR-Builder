using System;
using System.Collections.Generic;
using System.Linq;
using VRBuilder.Core.Exceptions;
using VRBuilder.Unity;

namespace VRBuilder.Core.SceneObjects
{
    public class NonUniqueSceneObjectRegistry : ISceneObjectRegistry
    {
        protected readonly Dictionary<Guid, List<ISceneObject>> registeredObjects = new Dictionary<Guid, List<ISceneObject>>();

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

        public bool ContainsName(string guidString)
        {
            Guid guid;

            if (Guid.TryParse(guidString, out guid))
            {
                return ContainsGuid(guid);
            }

            return false;
        }

        public ISceneObject GetByGuid(Guid guid)
        {
            if (registeredObjects.ContainsKey(guid))
            {
                return registeredObjects[guid].FirstOrDefault();
            }

            return null;
        }

        public ISceneObject GetByName(string name)
        {
            Guid guid;

            if (Guid.TryParse(name, out guid))
            {
                return GetByGuid(guid);
            }

            return null;
        }

        public IEnumerable<ISceneObject> GetByTag(Guid guid)
        {
            if (registeredObjects.ContainsKey(guid))
            {
                return registeredObjects[guid];
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
            if (obj == null)
            {
                throw new NullReferenceException("Attempted to register a null object.");
            }

            foreach (Guid tag in obj.AllTags)
            {
                RegisterTag(obj, tag);
            }

            obj.TagAdded += OnTagAdded;
            obj.TagRemoved += OnTagRemoved;
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

        public void RegisterAll()
        {
            //foreach (SceneObjectTags.Tag tag in SceneObjectTags.Instance.Tags)
            //{
            //    if (registeredObjects.ContainsKey(tag.Guid) == false)
            //    {
            //        registeredObjects.Add(tag.Guid, new List<ISceneObject>());
            //    }
            //}

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
                entity = registeredObjects[guid].First();
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

            if (obj == null)
            {
                // TODO exception
            }

            foreach (Guid tag in obj.AllTags)
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
    }
}
