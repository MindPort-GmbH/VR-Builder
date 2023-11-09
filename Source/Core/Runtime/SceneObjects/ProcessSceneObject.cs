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
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VRBuilder.Core.SceneObjects
{
    /// <inheritdoc cref="ISceneObject"/>
    [ExecuteInEditMode]
    public class ProcessSceneObject : MonoBehaviour, ISceneObject, ITagContainer
    {
        public event EventHandler<LockStateChangedEventArgs> Locked;
        public event EventHandler<LockStateChangedEventArgs> Unlocked;
        public GameObject GameObject => gameObject;

        [SerializeField]
        [Tooltip("Unique name which identifies an object in scene, can be null or empty, but has to be unique in the scene.")]
        [Obsolete("Support for ISceneObject.UniqueName will be removed with VR-Builder 4")]
        protected string uniqueName = "Obsolete with VR-Builder 4";

        /// <inheritdoc />
        [Obsolete("Support for ISceneObject.UniqueName will be removed with VR-Builder 4. Guid string is returned as name.", true)]
        public string UniqueName
        {
            get
            {
                return Guid.ToString();
            }
        }

        private const string NOT_INITIALIZED_GUID = "[not initialized guid]";
        [SerializeField]
        private string guid = NOT_INITIALIZED_GUID;

        private List<IStepData> unlockers = new List<IStepData>();

        /// <inheritdoc />
        public Guid Guid
        {
            get
            {
                Guid realGuid;
                if (!Guid.TryParse(guid, out realGuid))
                {
                    realGuid = Guid.NewGuid();
                    Debug.LogWarning($"Generated new real Guid {realGuid} instead of existing {guid}");

                    if (guid != NOT_INITIALIZED_GUID)
                    {
                        Debug.LogError($"Guid of GamObject {gameObject.name} had an invalid value {guid} resetting it to {realGuid}. Expect follow up issues");
                    }
                    guid = realGuid.ToString();
                }
                return realGuid;
            }
        }

        public ICollection<ISceneObjectProperty> Properties
        {
            get { return GetComponents<ISceneObjectProperty>(); }
        }

        public bool IsLocked { get; private set; }

        private bool IsRegistered
        {
            get { return RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(Guid); }
        }

        [SerializeField]
        protected List<string> tags = new List<string>();

        /// <inheritdoc />
        public IEnumerable<Guid> Tags => tags.Select(tag => Guid.Parse(tag));

        /// <inheritdoc />
        public event EventHandler<TaggableObjectEventArgs> TagAdded;

        /// <inheritdoc />
        public event EventHandler<TaggableObjectEventArgs> TagRemoved;

        protected void Awake()
        {
            Debug.Log($"Awake Finished for {this.name} from {Guid}");
        }

        protected void Start()
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

            Debug.Log($"Start Finished for {this.name} from {Guid}");
        }

        protected void OnEnable()
        {
            Debug.Log($"OnEnable {this.name}, instanceID {gameObject.GetInstanceID()} Guid {Guid}");
        }

        protected void OnDisable()
        {
            Debug.Log($"OnDisable {this.name}, instanceID {gameObject.GetInstanceID()} Guid {Guid}");
        }

        protected void Init()
        {
            //Debug.Log($"Init {this.name}, PlayModeTracker.IsPlayMode {PlayModeTracker.IsPlayMode}");

#if UNITY_EDITOR

            if (UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(gameObject.scene))
            {
                Debug.Log($"IsPreviewScene in {gameObject.name}. Returning");
                return;
            }
#endif

            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }

            //Ensure we have a valid guid
            _ = Guid;


            // TODO we might want to also create an override if this is the first prefab / prefab variant in the open scenes
            ISceneObject obj;
            if (RuntimeConfigurator.Configuration.SceneObjectRegistry.TryGetGuid(Guid, out obj))
            {
                if (obj.GameObject.GetInstanceID() == this.GameObject.GetInstanceID())
                {
                    Debug.Log($"Trying to register the same object twice {gameObject.name} - Guid {guid} - InstanceID {this.GameObject.GetInstanceID()} ");
                    return;
                }

                Debug.Log($"{gameObject.name} Guid {guid} already registered.");
                guid = Guid.NewGuid().ToString();
                Debug.Log($"Changed {gameObject.name} to guid {guid}");

#if UNITY_EDITOR
                if (PrefabUtility.IsPartOfPrefabInstance(this))
                {
                    var prefabInstance = PrefabUtility.GetOutermostPrefabInstanceRoot(this);
                    if (prefabInstance != null)
                    {
                        Debug.Log($"Recording prefab override {this.name} from {guid}");
                        PrefabUtility.RecordPrefabInstancePropertyModifications(prefabInstance);
                    }
                }
#endif
            }

            RuntimeConfigurator.Configuration.SceneObjectRegistry.Register(this);

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            Debug.Log($"Init Finished for {this.name} from {Guid}");
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR

            if (UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(gameObject.scene))
            {
                Debug.Log($"IsPreviewScene in {this.name}. Returning");
                return;
            }
#endif

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
                    Debug.LogErrorFormat("Property of type '{0}' is not attached to SceneObject '{1}' with realGuid {2}", propertyType.Name, gameObject.name, Guid);
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

        /// <inheritdoc />
        public void AddTag(Guid tag)
        {
            if (Tags.Contains(tag) == false)
            {
                tags.Add(tag.ToString());
                TagAdded?.Invoke(this, new TaggableObjectEventArgs(tag.ToString()));
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
                TagRemoved?.Invoke(this, new TaggableObjectEventArgs(tag.ToString()));
                return true;
            }

            return false;
        }
    }
}
