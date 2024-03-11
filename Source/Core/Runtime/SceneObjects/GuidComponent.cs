/// Guid based Reference copyright © 2018 Unity Technologies ApS
/// Licensed under the Unity Companion License for Unity-dependent projects--see 
/// Unity Companion License http://www.unity3d.com/legal/licenses/Unity_Companion_License.
/// Unless expressly provided otherwise, the Software under this license is made available strictly on an 
/// “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.

/// Modifications copyright (c) 2021-2023 MindPort GmbH

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
    public class GuidComponent : MonoBehaviour, ISerializationCallbackReceiver, ISceneObject
    {
        /// <summary>
        /// Unity's serialization system doesn't know about System.Guid, so we convert to a byte array
        /// Using strings allocates memory is was twice as slow
        /// </summary>
        [SerializeField]
        private byte[] serializedGuid;

        /// <summary>
        /// System guid we use for comparison and generation.
        /// When the <see cref="serializedGuid"/> is modified by the Unity editor 
        /// (e.g.: reverting a prefab) this will it possible ro revert the uniqueId.
        /// </summary>
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
                    if (IsSerializedGuildValid(serializedGuid))
                    {
                        guid = new Guid(serializedGuid);
                    }
                    else
                    {
                        SetUniqueId(Guid.NewGuid());
                    }
                }
                //Debug.Log($"Guid get: {guid}");

                //TODO this is not good we create a new guid every time we access this property
                //return new Guid(uniqueId);
                return guid;
            }
        }

        [Obsolete("Use Guid instead.")]
        /// <inheritdoc />
        public string UniqueName => Guid.ToString();

        [SerializeField]
        protected List<string> tags = new List<string>();

        /// <inheritdoc />
        public IEnumerable<Guid> Tags => tags.Select(tag => Guid.Parse(tag));

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
            var processSceneObjects = GetComponentsInChildren<GuidComponent>(true);
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
            // at a time that lets us detect what we are
            if (IsAssetOnDisk())
            {
                serializedGuid = null;
                guid = System.Guid.Empty;
            }
            //RuntimeConfigurator.Configuration.SceneObjectRegistry.Register(this);
            // // Revert changes done by editor prefab override revert
            // if (guid != Guid.Empty && guid != Guid)
            // {
            //     Debug.Log($"OnValidate: Reset uniqueId {serializedGuid} to {guid}");
            //     serializedGuid = guid.ToByteArray();
            // }
        }

        // We cannot allow a GUID to be saved into a prefab, and we want to convert to byte[]
        // This is called more often than you would think (e.g.: about once per frame if the object is selected in the editor)
        // - https://discussions.unity.com/t/onbeforeserialize-is-getting-called-rapidly/115546, 
        // - https://blog.unity.com/engine-platform/serialization-in-unity </remarks>
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
            if (guid != System.Guid.Empty)
            {
                serializedGuid = guid.ToByteArray();
            }
        }

        //TODO this is called before OnValidate when using the prefab override revert or apply
        /// <summary>
        /// On load, we can go head a restore our system guid for later use
        /// </summary>
        public void OnAfterDeserialize()
        {

            if (IsGuidAssigned())
            {
                //Happens when interacting with the prefab in the editor 

                // Editor prefab override revert all changes done by the user on the serializedGuid
                Debug.Log($"OnAfterDeserialize start: Editor override reverted serializedGuid: {GetSerializedGuidString()}, guid: {guid}");
                serializedGuid = guid.ToByteArray();
                Debug.Log($"OnAfterDeserialize end: Editor override reverted serializedGuid: {GetSerializedGuidString()}, guid: {guid}");
            }
            else if (IsSerializedGuildValid(serializedGuid))
            {
                // Drag and drop prefab into scene. We will check with the registry in Awake() for duplicate
                // Editor prefab override apply all changes done by the use
                Debug.Log($"OnAfterDeserialize start: Deserialized serializedGuid: {GetSerializedGuidString()}, guid: {guid}");
                guid = new Guid(serializedGuid);
                Debug.Log($"OnAfterDeserialize end: Deserialized serializedGuid: {GetSerializedGuidString()}, guid: {guid}");
            }
            else
            {
                // New GameObject we initialize guid lazy
                Debug.Log($"OnAfterDeserialize: No serializedGuid we initialize guid lazy guid: {guid}");
            }
        }

        private string GetSerializedGuidString()
        {
            if (serializedGuid == null || serializedGuid.Length == 0)
            {
                return "null";
            }
            else if (serializedGuid.Length != 16)
            {
                return "invalid";
            }
            else
            {
                return new Guid(serializedGuid).ToString();
            }
        }

        // TODO: Test This
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

            serializedGuid = null;
            tags = new List<string>();
            Init();

            EditorUtility.SetDirty(this);
        }

        // TODO: Test This
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

                    serializedGuid = null;
                    Init();

                    EditorUtility.SetDirty(this);
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
            Debug.Log($"SetUniqueId: {guid}");
            serializedGuid = guid.ToByteArray();
            this.guid = guid;
        }

        [Obsolete("This is no longer supported.")]
        public void ChangeUniqueName(string newName = "")
        {

        }

        public bool IsGuidAssigned()
        {
            return guid != System.Guid.Empty;
        }

        //TODO move in to helper class
        /// <summary>
        /// Checks if the serialized GUID is valid.
        /// </summary>
        /// <param name="serializedGuid">The serialized GUID to check.</param>
        /// <returns>True if the serialized GUID is not null and has a length of 16; otherwise, false.</returns>
        public static bool IsSerializedGuildValid(byte[] serializedGuid)
        {
            return serializedGuid != null && serializedGuid.Length == 16;
        }

        /// <summary>
        /// Initializes the GuidComponent by registering it with the SceneObjectRegistry.
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
            //TODO What is this for exactly? Where to put this?
            // If we are creating a new GUID for a prefab instance of a prefab, but we have somehow lost our prefab connection
            // force a save of the modified prefab instance properties
            if (PrefabUtility.IsPartOfNonAssetPrefabInstance(this))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            }
#endif
            //}
            // else if (cachedGuid == System.Guid.Empty)
            // {
            //     // otherwise, we should set our system guid to our serialized guid
            //     cachedGuid = new System.Guid(uniqueId);
            // }

            // register with the GUID Manager so that other components can access this
            //if (cachedGuid != System.Guid.Empty)
            //{
            RuntimeConfigurator.Configuration.SceneObjectRegistry.Register(this);
            //}
        }

#if UNITY_EDITOR

        public bool IsAssetOnDisk()
        {
            // Happens when in prefab mode and adding or removing components
            if (this == null)
            {
                return false;
            }

            bool isPartOfPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(this);
            if (isPartOfPrefabAsset)
            {
                return true;
            }

            bool isEditingInPrefabMode = IsEditingInPrefabMode();
            if (isEditingInPrefabMode)
            {
                return true;
            }
            return false;

            //return PrefabUtility.IsPartOfPrefabAsset(this) || IsEditingInPrefabMode();
        }
        private bool IsEditingInPrefabMode()
        {
            if (EditorUtility.IsPersistent(this))
            {
                // if the game object is stored on disk, it is a prefab of some kind, despite not returning true for IsPartOfPrefabAsset =/
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
                        return true;
                    }
                }
            }
            return false;
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

        /// <inheritdoc />
        public override string ToString()
        {
            return GameObject.name;
        }
    }
}
