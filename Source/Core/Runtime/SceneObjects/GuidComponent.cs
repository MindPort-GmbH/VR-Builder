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
        private SerializableGuid serializedGuid;

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
                    if (SerializableGuid.IsValid(serializedGuid))
                    {
                        guid = serializedGuid.Guid;
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
        protected List<SerializableGuid> tags = new List<SerializableGuid>();

        /// <inheritdoc />
        public IEnumerable<Guid> Tags => tags.Select(tagBytes => tagBytes.Guid);

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
                serializedGuid.SetGuid(guid);
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
                Debug.Log($"OnAfterDeserialize start: Editor override reverted serializedGuid: {serializedGuid}, guid: {guid}");
                serializedGuid.SetGuid(guid);
                Debug.Log($"OnAfterDeserialize end: Editor override reverted serializedGuid: {serializedGuid}, guid: {guid}");
            }
            else if (SerializableGuid.IsValid(serializedGuid))
            {
                // Drag and drop prefab into scene. We will check with the registry in Awake() for duplicate
                // Editor prefab override apply all changes done by the use
                Debug.Log($"OnAfterDeserialize start: Deserialized serializedGuid: {serializedGuid}, guid: {guid}");
                guid = serializedGuid.Guid;
                Debug.Log($"OnAfterDeserialize end: Deserialized serializedGuid: {serializedGuid}, guid: {guid}");
            }
            else
            {
                // New GameObject we initialize guid lazy
                Debug.Log($"OnAfterDeserialize: No serializedGuid we initialize guid lazy guid: {guid}");
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
            tags = new List<SerializableGuid>();
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
            serializedGuid.SetGuid(guid);
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
            RuntimeConfigurator.Configuration.SceneObjectRegistry.Register(this);
        }

#if UNITY_EDITOR
        //TODO Possibly move in to Helper class
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

        private bool wasInPrefabMode = false;

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
                EditorApplication.delayCall += CheckMainStageTransition;
            }
            wasInPrefabMode = false;
            return false;
        }

        private void CheckMainStageTransition()
        {
            // Determine if we are back in the main scene
            if (StageUtility.GetCurrentStageHandle() == StageUtility.GetMainStageHandle())
            {
                //Debug.Log($"We are back in the main scene: guid: {guid}, gameObject: {gameObject.name}");
                //EditorApplication.delayCall += RegAll;
                SceneObjectRegistryV2 reg = RuntimeConfigurator.Configuration.SceneObjectRegistry as SceneObjectRegistryV2;
                reg.DebugRebuild();
                //TODO wee need to set the current selected competent dirty is its a PSO
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
