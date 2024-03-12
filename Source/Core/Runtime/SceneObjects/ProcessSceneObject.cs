/// Guid based Reference copyright © 2018 Unity Technologies ApS
/// Licensed under the Unity Companion License for Unity-dependent projects--see 
/// Unity Companion License http://www.unity3d.com/legal/licenses/Unity_Companion_License.
/// Unless expressly provided otherwise, the Software under this license is made available strictly on an 
/// “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.

/// Modifications copyright (c) 2021-2024 MindPort GmbH

using System.Collections.Generic;
using UnityEngine;
using VRBuilder.Core.Configuration;
using System;
using VRBuilder.Core.Properties;
using VRBuilder.Core.Exceptions;
using System.Linq;
using VRBuilder.Core.Utils.Logging;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace VRBuilder.Core.SceneObjects
{
    // This component gives a GameObject a stable, non-replicatable Globally Unique IDentifier.
    // It can be used to reference a specific instance of an object no matter where it is.
    // This can also be used for other systems, such as Save/Load game
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
        private Guid guid = Guid.Empty;

        [SerializeField]
        [Tooltip("Unique name which identifies an object in scene, can be null or empty, but has to be unique in the scene.")]
        [Obsolete("This exists for backwards compatibility. Use the uniqueId field to store the object's unique identifier.")]
        protected string uniqueName = null;

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
                        SetUniqueId(Guid.NewGuid());
                    }
                }
                return guid;
            }
        }

        [Obsolete("Use Guid instead.")]
        /// <inheritdoc />
        public string UniqueName => Guid.ToString();

        [SerializeField]
        protected List<SerializableGuid> tags = new List<SerializableGuid>();

        /// <inheritdoc />
        public IEnumerable<Guid> Tags => tags.Select(tagBytes => tagBytes.Guid);

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

        [Obsolete("This event is no longer used and will be removed in the next major release.")]
#pragma warning disable CS0067 //The event 'event' is never used
        public event EventHandler<SceneObjectNameChanged> UniqueNameChanged;
#pragma warning restore CS0067
        public event EventHandler<LockStateChangedEventArgs> Locked;
        public event EventHandler<LockStateChangedEventArgs> Unlocked;
        public event EventHandler<TaggableObjectEventArgs> TagAdded;
        public event EventHandler<TaggableObjectEventArgs> TagRemoved;

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

#if UNITY_EDITOR
        private void OnValidate()
        {
            // similar to on Serialize, but gets called on Copying a Component or Applying a Prefab
            if (IsAssetOnDisk())
            {
                serializedGuid = null;
                guid = System.Guid.Empty;
            }
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
            if (IsAssetOnDisk())
            {
                serializedGuid = null;
                guid = System.Guid.Empty;
                return;
            }

#endif
            if (IsGuidAssigned() && !serializedGuid.Equals(guid))
            {
                serializedGuid.SetGuid(guid);
#if UNITY_EDITOR
                //FIXME SetDirty does not work guid string is not updated
                EditorUtility.SetDirty(this);
#endif
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

        /// <summary>
        /// Overriding the Reset context menu entry in order to unregister the object before invalidating the unique id.
        /// </summary>
        [ContextMenu("Reset", false, 0)]
        protected void ResetContextMenu()
        {
            if (RuntimeConfigurator.Exists)
            {
                RuntimeConfigurator.Configuration.SceneObjectRegistry.Unregister(this);
            }

            // On Reset, we want to generate a new Guid
            SetUniqueId(Guid.NewGuid());
            tags = new List<SerializableGuid>();
            Init();
        }

        [ContextMenu("Reset Unique ID")]
        protected void MakeUnique()
        {
            if (EditorUtility.DisplayDialog("Reset Unique Id", "Warning! This will change the object's unique id.\n" +
                "All reference to this object in the Process Editor will become invalid.\n" +
                "Proceed?", "Yes", "No"))
            {
                if (RuntimeConfigurator.Exists)
                {
                    RuntimeConfigurator.Configuration.SceneObjectRegistry.Unregister(this);

                    SetUniqueId(Guid.NewGuid());
                    Init();
                }
            }
        }
#endif

        private void OnDestroy()
        {
            if (RuntimeConfigurator.Exists)
            {
                RuntimeConfigurator.Configuration.SceneObjectRegistry.Unregister(this);
            }
        }

        /// <inheritdoc />
        public void SetUniqueId(Guid guid)
        {
            Undo.RecordObject(this, "Changed GUID");
            serializedGuid.SetGuid(guid);
            this.guid = guid;
#if UNITY_EDITOR
            //FIXME SetDirty does not work guid string is not updated
            EditorUtility.SetDirty(this);
#endif
        }

        [Obsolete("This is no longer supported.")]
        public void ChangeUniqueName(string newName = "") { }

        /// <summary>
        /// Checks if the Guid was assigned a value and not <c>System.Guid.Empty</c>.
        /// </summary>
        /// <returns><c>true</c> if the Guid is assigned; otherwise, <c>false</c>.</returns>
        protected bool IsGuidAssigned()
        {
            return guid != System.Guid.Empty;
        }

        /// <summary>
        /// Initializes the ProcessSceneObject by registering it with the SceneObjectRegistry.
        /// It will not register if in prefab mode edit mode or if we are a prefab asset.
        /// </summary>
        protected void Init()
        {
            if (RuntimeConfigurator.Exists == false)
            {
                Debug.LogWarning($"Not registering {gameObject.name} due to runtime configurator not present.");
                return;
            }

#if UNITY_EDITOR
            // if in editor, make sure we aren't a prefab of some kind
            if (IsAssetOnDisk())
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
        //TODO Possibly move this in to helper class
        /// <summary>
        /// Checks if the asset is saved on disk (e.g: a prefab in edit mode).
        /// </summary>
        /// <returns><c>true</c> if the asset is saved on disk; otherwise, <c>false</c>.</returns>
        public bool IsAssetOnDisk()
        {
            // Happens when in prefab mode and adding or removing components
            if (this == null)
            {
                return false;
            }

            return PrefabUtility.IsPartOfPrefabAsset(this) || IsEditingInPrefabMode();
        }

        private bool wasInPrefabMode = false;

        /// <summary>
        /// Determines whether the current object is being edited in prefab mode.
        /// </summary>
        /// <returns><c>true</c> if the object is being edited in prefab mode; otherwise, <c>false</c>.</returns>
        private bool IsEditingInPrefabMode()
        {
            if (EditorUtility.IsPersistent(this))
            {
                // if the game object is stored on disk, it is a prefab of some kind, despite not returning true for IsPartOfPrefabAsset =/
                wasInPrefabMode = true;
                return true;
            }
            else
            {
                // If the GameObject is not persistent let's determine which stage we are in first because getting Prefab info depends on it
                var mainStage = StageUtility.GetMainStageHandle();
                var currentStage = StageUtility.GetStageHandle(gameObject);
                if (currentStage != mainStage)
                {
                    var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
                    if (prefabStage != null)
                    {
                        wasInPrefabMode = true;
                        return true;
                    }
                }
            }
            if (wasInPrefabMode)
            {
                CheckMainStageTransition();
            }
            wasInPrefabMode = false;
            return false;
        }

        private void CheckMainStageTransition()
        {
            //Make sure we are in back in the main scene and not left a nester prefab stage
            if (StageUtility.GetCurrentStageHandle() == StageUtility.GetMainStageHandle())
            {
                //FIXME We need to update the SceneObjectRegistry because we do not do it in prefab edit mode
                // it is possible that PSO as well as Tags have been added or removed
                SceneObjectRegistryV2 reg = RuntimeConfigurator.Configuration.SceneObjectRegistry as SceneObjectRegistryV2;
                reg.DebugRebuild();
                //TODO wee need to set the current selected competent dirty if its a PSO
            }
        }
#endif

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
            var serializableTag = new SerializableGuid(tag.ToByteArray());
            if (!HasTag(tag))
            {
                tags.Add(serializableTag);
                TagAdded?.Invoke(this, new TaggableObjectEventArgs(tag));
            }
        }

        /// <inheritdoc />
        public bool HasTag(Guid tag)
        {
            return tags.Any(serializableTag => serializableTag.Equals(tag));
        }

        /// <inheritdoc />
        public bool RemoveTag(Guid tag)
        {
            var serializableTag = tags.FirstOrDefault(t => t.Equals(tag));
            if (serializableTag != null)
            {
                tags.Remove(serializableTag);
                TagRemoved?.Invoke(this, new TaggableObjectEventArgs(tag));
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
