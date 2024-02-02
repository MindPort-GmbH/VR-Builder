// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Exceptions;
using VRBuilder.Core.Properties;
using VRBuilder.Core.Utils.Logging;

namespace VRBuilder.Core.SceneObjects
{
    /// <inheritdoc cref="ISceneObject"/>
    [ExecuteInEditMode]
    public class ProcessSceneObject : MonoBehaviour, ISceneObject
    {
        public event EventHandler<LockStateChangedEventArgs> Locked;
        public event EventHandler<LockStateChangedEventArgs> Unlocked;
        public event EventHandler<SceneObjectNameChanged> UniqueNameChanged;
        public GameObject GameObject => gameObject;

        [SerializeField]
        [Tooltip("Unique name which identifies an object in scene, can be null or empty, but has to be unique in the scene.")]
        protected string uniqueId = null;

        /// <inheritdoc />
        public string UniqueName => Guid.ToString();

        private List<IStepData> unlockers = new List<IStepData>();

        /// <inheritdoc />
        public Guid Guid
        {
            get
            {
                if (uniqueId == null || Guid.TryParse(uniqueId, out Guid guid) == false)
                {
                    uniqueId = Guid.NewGuid().ToString();
                }

                return Guid.Parse(uniqueId);
            }
        }

        public ICollection<ISceneObjectProperty> Properties
        {
            get { return GetComponents<ISceneObjectProperty>(); }
        }

        public bool IsLocked { get; private set; }

        [SerializeField]
        protected List<string> tags = new List<string>();

        /// <inheritdoc />
        public IEnumerable<Guid> Tags => tags.Select(tag => Guid.Parse(tag));

        /// <inheritdoc />
        public IEnumerable<Guid> AllTags => new List<Guid>() { Guid }.Concat(Tags);

        /// <inheritdoc />
        public event EventHandler<TaggableObjectEventArgs> TagAdded;

        /// <inheritdoc />
        public event EventHandler<TaggableObjectEventArgs> TagRemoved;

        protected void Awake()
        {
            Init();

            var processSceneObjects = GetComponentsInChildren<ProcessSceneObject>(true);
            for (int i = 0; i < processSceneObjects.Length; i++)
            {
                if (!processSceneObjects[i].isActiveAndEnabled)
                {
                    processSceneObjects[i].Init();
                }
            }
        }

        private void Reset()
        {
            Init();
        }

        protected void Init()
        {
#if UNITY_EDITOR
            if (UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(gameObject.scene))
            {
                Debug.LogError($"Not registering {gameObject.name} due to preview scene.");
                return;
            }
#endif
            if (RuntimeConfigurator.Exists == false)
            {
                Debug.LogError($"Not registering {gameObject.name} due to runtime configurator not present.");
                return;
            }

            if (IsDuplicateUniqueTag())
            {
                uniqueId = Guid.NewGuid().ToString();
            }

            RuntimeConfigurator.Configuration.SceneObjectRegistry.Register(this);
        }

        private bool IsDuplicateUniqueTag()
        {
            if (RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(Guid) == false)
            {
                return false;
            }

            IEnumerable<ISceneObject> sceneObjects = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(Guid);

            return sceneObjects.Contains(this) == false;
        }

        private void OnDestroy()
        {
            if (RuntimeConfigurator.Exists)
            {
                RuntimeConfigurator.Configuration.SceneObjectRegistry.Unregister(this);
            }
        }

        public bool CheckHasProperty<T>() where T : ISceneObjectProperty
        {
            return CheckHasProperty(typeof(T));
        }

        public bool CheckHasProperty(Type type)
        {
            return FindProperty(type) != null;
        }

        public T GetProperty<T>() where T : ISceneObjectProperty
        {
            ISceneObjectProperty property = FindProperty(typeof(T));
            if (property == null)
            {
                throw new PropertyNotFoundException(this, typeof(T));
            }

            return (T)property;
        }

        public void ValidateProperties(IEnumerable<Type> properties)
        {
            bool hasFailed = false;
            foreach (Type propertyType in properties)
            {
                // ReSharper disable once InvertIf
                if (CheckHasProperty(propertyType) == false)
                {
                    Debug.LogErrorFormat("Property of type '{0}' is not attached to SceneObject '{1}'", propertyType.Name, gameObject.name);
                    hasFailed = true;
                }
            }

            if (hasFailed)
            {
                throw new PropertyNotFoundException("One or more SceneObjectProperties could not be found, check your log entries for more information.");
            }
        }

        public void SetLocked(bool lockState)
        {
            if (IsLocked == lockState)
            {
                return;
            }

            IsLocked = lockState;

            if (IsLocked)
            {
                if (Locked != null)
                {
                    Locked.Invoke(this, new LockStateChangedEventArgs(IsLocked));
                }
            }
            else
            {
                if (Unlocked != null)
                {
                    Unlocked.Invoke(this, new LockStateChangedEventArgs(IsLocked));
                }
            }
        }

        /// <inheritdoc/>
        public virtual void RequestLocked(bool lockState, IStepData stepData)
        {
            if (lockState == false && unlockers.Contains(stepData) == false)
            {
                unlockers.Add(stepData);
            }

            if (lockState && unlockers.Contains(stepData))
            {
                unlockers.Remove(stepData);
            }

            bool canLock = unlockers.Count == 0;

            if (LifeCycleLoggingConfig.Instance.LogLockState)
            {
                string lockType = lockState ? "lock" : "unlock";
                string requester = stepData == null ? "NULL" : stepData.Name;
                StringBuilder unlockerList = new StringBuilder();

                foreach (IStepData unlocker in unlockers)
                {
                    unlockerList.Append($"\n<i>{unlocker.Name}</i>");
                }

                string listUnlockers = unlockers.Count == 0 ? "" : $"\nSteps keeping this object unlocked:{unlockerList}";

                Debug.Log($"<i>{this.GetType().Name}</i> on <i>{gameObject.name}</i> received a <b>{lockType}</b> request from <i>{requester}</i>." +
                    $"\nCurrent lock state: <b>{IsLocked}</b>. Future lock state: <b>{lockState && canLock}</b>{listUnlockers}");
            }

            SetLocked(lockState && canLock);
        }

        /// <inheritdoc/>
        public bool RemoveUnlocker(IStepData data)
        {
            return unlockers.Remove(data);
        }

        /// <summary>
        /// Tries to find property which is assignable to given type, this method
        /// will return null if none is found.
        /// </summary>
        private ISceneObjectProperty FindProperty(Type type)
        {
            return GetComponent(type) as ISceneObjectProperty;
        }

        public void ChangeUniqueName(string newName = "")
        {
            if (RuntimeConfigurator.Exists)
            {
                RuntimeConfigurator.Configuration.SceneObjectRegistry.Unregister(this);
            }

            uniqueId = Guid.NewGuid().ToString();

            if (RuntimeConfigurator.Exists)
            {
                RuntimeConfigurator.Configuration.SceneObjectRegistry.Register(this);
            }
        }

        /// <inheritdoc />
        public void AddTag(Guid tag)
        {
            if (Tags.Contains(tag) == false)
            {
                tags.Add(tag.ToString());
                TagAdded?.Invoke(this, new TaggableObjectEventArgs(tag));
            }
        }

        /// <inheritdoc />
        public bool HasTag(Guid tag)
        {
            return Tags.Contains(tag);
        }

        /// <inheritdoc />
        public bool RemoveTag(Guid tag)
        {
            if (tags.Remove(tag.ToString()))
            {
                TagRemoved?.Invoke(this, new TaggableObjectEventArgs(tag));
                return true;
            }

            return false;
        }
    }
}
