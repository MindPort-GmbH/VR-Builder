// Guid based Reference copyright 2018 Unity Technologies ApS
// Licensed under the Unity Companion License for Unity-dependent projects--see 
// Unity Companion License http://www.unity3d.com/legal/licenses/Unity_Companion_License.
// Unless expressly provided otherwise, the Software under this license is made available strictly on an 
// AS IS BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.

// Modifications copyright (c) 2021-2025 MindPort GmbH
// Licensed under the Apache License, Version 2.0

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
using VRBuilder.Unity;
#endif

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// This component gives a GameObject a stable, non-replicatable Globally Unique Identifier.
    /// It can be used to reference a specific instance of an object no matter where it is.  
    /// </summary>
    [ExecuteInEditMode, DisallowMultipleComponent]
    public class ProcessSceneObject : MonoBehaviour, ISerializationCallbackReceiver, ISceneObject
    {
        /// <summary>
        /// Unity's serialization system doesn't know about System.Guid, so we convert to a byte array
        /// Using strings allocates memory is was twice as slow
        /// </summary>
        [SerializeField]
        private SerializableGuid serializedGuid;

        /// <summary>
        /// We use this Guid for comparison, generation and caching.
        /// </summary>
        /// <remarks> 
        /// When the <see cref="serializedGuid"/> is modified by the Unity editor 
        /// (e.g.: reverting a prefab) this will be used to revert it back canaling the changes of the editor.
        /// </remarks>
        protected Guid guid = Guid.Empty;

        /// <inheritdoc />
        public Guid Guid
        {
            get
            {
                if (!IsGuidAssigned())
                {
                    // if our serialized data is invalid, then we are a new object and need a new GUID
                    if (SerializableGuid.IsValid(serializedGuid))
                    {
                        guid = serializedGuid.Guid;
                    }
                    else
                    {
                        SetObjectId(Guid.NewGuid());
                    }
                }
                return guid;
            }
        }

        [SerializeField]
        protected List<SerializableGuid> guids = new List<SerializableGuid>();

        /// <inheritdoc />
        public IEnumerable<Guid> Guids => guids.Select(bytes => bytes.Guid);

        /// <inheritdoc />
        public GameObject GameObject => gameObject;

        /// <summary>
        /// Properties associated with this scene object.
        /// </summary>
        public ICollection<ISceneObjectProperty> Properties
        {
            get { return GetComponents<ISceneObjectProperty>(); }
        }

        private List<IStepData> unlockers = new List<IStepData>();

        /// <inheritdoc />
        public bool IsLocked { get; private set; }

        public event EventHandler<LockStateChangedEventArgs> Locked;
        public event EventHandler<LockStateChangedEventArgs> Unlocked;
        public event EventHandler<GuidContainerEventArgs> GuidAdded;
        public event EventHandler<GuidContainerEventArgs> GuidRemoved;
        public event EventHandler<UniqueIdChangedEventArgs> ObjectIdChanged;

        private void Awake()
        {
            Init();

            // Register inactive ProcessSceneObjects
            var processSceneObjects = GetComponentsInChildren<ProcessSceneObject>(true);
            for (int i = 0; i < processSceneObjects.Length; i++)
            {
                if (!processSceneObjects[i].isActiveAndEnabled)
                {
                    processSceneObjects[i].Init();
                }
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            // similar to OnSerialize, but gets called on Copying a Component or Applying a Prefab
            if (!IsInTheScene())
            {
                // This catches all cases adding, removing, creating, deleting
                // But it adds overhead e.g. it is also called when entering prefab edit mode or entering the scene
                if (RuntimeConfigurator.Exists)
                {
                    RuntimeConfigurator.MarkSceneObjectDirty(this);
                }
                SetGuidDefaultValues();
            }
#endif
        }

        /// <summary>
        /// Implement this method to receive a callback before Unity serializes your object.
        /// </summary> 
        /// <remarks>
        /// We use this to prevent the GUID to be saved into a prefab on disk.
        /// Be aware this is called more often than you would think (e.g.: about once per frame if the object is selected in the editor)
        /// - https://discussions.unity.com/t/onbeforeserialize-is-getting-called-rapidly/115546, 
        /// - https://blog.unity.com/engine-platform/serialization-in-unity </remarks>
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            // This lets us detect if we are a prefab instance or a prefab asset.
            // A prefab asset cannot contain a GUID since it would then be duplicated when instanced.
            if (!IsInTheScene())
            {
                SetGuidDefaultValues();
                return;
            }
#endif
            if (IsGuidAssigned() && !serializedGuid.Equals(guid))
            {
                Guid previousGuid = Guid;
                serializedGuid.SetGuid(guid);

                ObjectIdChanged?.Invoke(this, new UniqueIdChangedEventArgs(previousGuid, Guid));
            }
        }

        /// <summary>
        /// Implement this method to receive a callback after Unity deserializes your object.
        /// </summary>
        /// <remarks>
        /// We use this to restore the <see cref="serializedGuid"/> when it was unwanted changed by the editor 
        /// or assign <see cref="guid"> from the stored <see cref="serializedGuid"/>.
        /// </remarks>
        public void OnAfterDeserialize()
        {
            if (IsGuidAssigned())
            {
                /// Restore Guid:
                /// - Editor Prefab Overrides -> Revert
                serializedGuid.SetGuid(guid);
            }
            else if (SerializableGuid.IsValid(serializedGuid))
            {
                /// Apply Serialized Guid:
                /// - Open scene
                /// - Recompile
                /// - Editor Prefab Overrides -> Apply
                /// - Start Playmode
                guid = serializedGuid.Guid;
            }
            else
            {
                /// - New GameObject we initialize guid lazy
                /// - Drag and drop prefab into scene
                /// - Interacting with the prefab outside of the scene
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Overriding the Reset context menu entry in order to unregister the object before invalidating the object ID.
        /// </summary>
        [ContextMenu("Reset", false, 0)]
        protected void ResetContextMenu()
        {
            if (RuntimeConfigurator.Exists)
            {
                RuntimeConfigurator.Configuration.SceneObjectRegistry.Unregister(this);
            }

            // On Reset, we want to generate a new Guid
            SetObjectId(Guid.NewGuid());
            guids = new List<SerializableGuid>();
            Init();
        }

        [ContextMenu("Reset Object ID")]
        protected void MakeUnique()
        {
            if (EditorUtility.DisplayDialog("Reset Object ID", "Warning! This will change the object's unique ID.\n" +
                "All reference to this object in the Process Editor will become invalid.\n" +
                "Proceed?", "Yes", "No"))
            {
                ResetUniqueId();
            }
        }
#endif
        public void ResetUniqueId()
        {
            if (RuntimeConfigurator.Exists)
            {
                RuntimeConfigurator.Configuration.SceneObjectRegistry.Unregister(this);

                SetObjectId(Guid.NewGuid());
                Init();
            }
        }

        private void OnDestroy()
        {
            if (RuntimeConfigurator.Exists)
            {
                RuntimeConfigurator.Configuration.SceneObjectRegistry.Unregister(this);
            }
        }

        /// <inheritdoc />
        public void SetObjectId(Guid guid)
        {
            Guid previousGuid = serializedGuid != null && serializedGuid.IsValid() ? serializedGuid.Guid : Guid.Empty;
#if UNITY_EDITOR
            Undo.RecordObject(this, "Changed GUID");
#endif
            serializedGuid.SetGuid(guid);
            this.guid = guid;

            ObjectIdChanged?.Invoke(this, new UniqueIdChangedEventArgs(previousGuid, Guid));
        }

        /// <summary>
        /// Checks if the Guid was assigned a value and not <c>System.Guid.Empty</c>.
        /// </summary>
        /// <returns><c>true</c> if the Guid is assigned; otherwise, <c>false</c>.</returns>
        protected bool IsGuidAssigned()
        {
            return guid != Guid.Empty;
        }

        /// <summary>
        /// Initializes the ProcessSceneObject by registering it with the SceneObjectRegistry.
        /// It will not register if in prefab mode edit mode or if we are a prefab asset.
        /// </summary>
        protected void Init()
        {
            if (!RuntimeConfigurator.Exists)
            {
                return;
            }

#if UNITY_EDITOR
            // if in editor, make sure we aren't a prefab of some kind
            if (!IsInTheScene())
            {
                return;
            }
#endif

#if UNITY_EDITOR
            //TODO This is from the Unity code for some edge case I do not know about yet
            // If we are creating a new GUID for a prefab instance of a prefab, but we have somehow lost our prefab connection
            // force a save of the modified prefab instance properties
            // if (PrefabUtility.IsPartOfNonAssetPrefabInstance(this))
            // {
            //     PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            // }
#endif
            RuntimeConfigurator.Configuration.SceneObjectRegistry.Register(this);
        }

#if UNITY_EDITOR

        /// <summary>
        /// Checks if the current object is in the scene and tracks stage transitions.
        /// </summary>
        /// <returns><c>true</c> if the object is in the scene; otherwise, <c>false</c>.</returns>
        private bool IsInTheScene()
        {
            bool isSceneObject = AssetUtility.IsComponentInScene(this);
            return isSceneObject;
        }
#endif

        /// <summary>
        /// Sets the default values for the serializedGuid and guid variables.
        /// </summary>
        private void SetGuidDefaultValues()
        {
            serializedGuid = null;
            guid = System.Guid.Empty;
        }

        /// <inheritdoc />
        public bool CheckHasProperty<T>() where T : ISceneObjectProperty
        {
            return CheckHasProperty(typeof(T));
        }

        /// <inheritdoc />
        public bool CheckHasProperty(Type type)
        {
            return FindProperty(type) != null;
        }

        /// <inheritdoc />
        public T GetProperty<T>() where T : ISceneObjectProperty
        {
            ISceneObjectProperty property = FindProperty(typeof(T));
            if (property == null)
            {
                throw new PropertyNotFoundException(this, typeof(T));
            }

            return (T)property;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void SetLocked(bool lockState)
        {
            if (IsLocked == lockState)
            {
                return;
            }

            IsLocked = lockState;

            if (IsLocked)
            {
                Locked?.Invoke(this, new LockStateChangedEventArgs(IsLocked));
            }
            else
            {
                Unlocked?.Invoke(this, new LockStateChangedEventArgs(IsLocked));
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
        
        /// <inheritdoc/>
        public bool IsAlwaysUnlocked { get; set; }

        /// <summary>
        /// Tries to find a property which is assignable to a given type, this method
        /// will return null if none is found.
        /// </summary>
        private ISceneObjectProperty FindProperty(Type type)
        {
            return GetComponent(type) as ISceneObjectProperty;
        }

        /// <inheritdoc />
        public void AddGuid(Guid guid)
        {
            var serializableGuid = new SerializableGuid(guid.ToByteArray());
            if (!HasGuid(guid))
            {
                guids.Add(serializableGuid);
                GuidAdded?.Invoke(this, new GuidContainerEventArgs(guid));
            }
        }

        /// <inheritdoc />
        public bool HasGuid(Guid guid)
        {
            return guids.Any(serializableGuid => serializableGuid.Equals(guid));
        }

        /// <inheritdoc />
        public bool RemoveGuid(Guid guid)
        {
            var serializableGuid = guids.FirstOrDefault(t => t.Equals(guid));
            if (serializableGuid != null)
            {
                guids.Remove(serializableGuid);
                GuidRemoved?.Invoke(this, new GuidContainerEventArgs(guid));
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return GameObject.name;
        }
    }
}
