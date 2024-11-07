using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.SceneManagement;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.ProcessAssets;
using VRBuilder.Core.Editor.ProcessUpgradeTool.Converters;
using VRBuilder.Core.Editor.ProcessUpgradeTool.Updaters;
using VRBuilder.Core.EntityOwners;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;
using VRBuilder.Unity;

namespace VRBuilder.Core.Editor.ProcessUpgradeTool
{
    /// <summary>
    /// Tool for upgrading an old process loaded in a scene to be compatible with the latest version of VR Builder.
    /// </summary>
    public static class ProcessUpgradeTool
    {
        private static IEnumerable<IUpdater> updaters;
        private static IEnumerable<IConverter> converters;

        /// <summary>
        /// Updaters available to this tool.
        /// </summary>
        public static IEnumerable<IUpdater> Updaters
        {
            get
            {
                if (updaters == null)
                {
                    updaters = new List<IUpdater>(ReflectionUtils.GetConcreteImplementationsOf<IUpdater>()
                        .Select(ReflectionUtils.CreateInstanceOfType)
                        .Cast<IUpdater>());
                }

                return updaters;
            }
        }

        /// <summary>
        /// Converters available to this tool.
        /// </summary>
        public static IEnumerable<IConverter> Converters
        {
            get
            {
                if (converters == null)
                {
                    converters = new List<IConverter>(ReflectionUtils.GetConcreteImplementationsOf<IConverter>()
                        .Select(ReflectionUtils.CreateInstanceOfType)
                        .Cast<IConverter>());
                }

                return converters;
            }
        }

        //[MenuItem("Tools/VR Builder/Developer/Fix Groups", false, 70)]
        // Used to fix GroupsToUnlock == null errors.
        private static void FixGroups()
        {
            IProcess process = GlobalEditorHandler.GetCurrentProcess();

            foreach (IChapter chapter in process.Data.Chapters)
            {
                foreach (IStep step in chapter.Data.Steps)
                {
                    FixGroupsToUnlockInStep(step);
                }
            }

            ProcessAssetManager.Save(process);
        }

        private static void FixGroupsToUnlockInStep(IStep step)
        {
            if (((Step)step).Data.GroupsToUnlock == null)
            {
                ((Step)step).Data.GroupsToUnlock = new Dictionary<Guid, IEnumerable<Type>>();
            }

            foreach (IBehavior behavior in step.Data.Behaviors.Data.Behaviors)
            {
                if (behavior is IDataOwner dataOwner && dataOwner.Data is EntityCollectionData<IChapter> nestedData)
                {
                    foreach (IChapter chapter in nestedData.GetChildren())
                    {
                        foreach (IStep nestedStep in chapter.Data.Steps)
                        {
                            FixGroupsToUnlockInStep(nestedStep);
                        }
                    }
                }
            }
        }

        //        [MenuItem("Tools/VR Builder/Developer/Update Process in Scene", false, 70)]
        // Was used for updating from VR Builder 3 to VR Builder 4.
        private static void UpdateProcessMenuEntry()
        {
            if (RuntimeConfigurator.Exists == false)
            {
                UnityEngine.Debug.LogError("This is not a VR Builder scene");
                return;
            }

            IProcess process = GlobalEditorHandler.GetCurrentProcess();

            if (process == null)
            {
                UnityEngine.Debug.LogError("No active process found.");
                return;
            }

            if (EditorUtility.DisplayDialog("Process Upgrade Tool", $"Updating the current process to the newest version of VR Builder.\n" +
                $"The correct scene needs to be opened for the upgrade to work properly.\n\n" +
                $"Process: {process.Data.Name}\n" +
                $"Scene: {SceneManager.GetActiveScene().name}\n\n" +
                $"Ensure you have backed up your process file before proceeding!\n" +
                $"Continue?", "Ok", "Cancel") == false)
            {
                return;
            }

            IEnumerable<ProcessSceneObject> processSceneObjects = SceneUtils.GetActiveAndInactiveComponents<ProcessSceneObject>();
            foreach (ProcessSceneObject sceneObject in processSceneObjects)
            {
                sceneObject.ResetUniqueId();
                EditorUtility.SetDirty(sceneObject);
            }

            UpdateDataOwnerRecursively(process);

            ProcessAssetManager.Save(process);
        }

        /// <summary>
        /// Updates the provided data owner and all of its child data owners, if a suitable updater exists.
        /// </summary>
        public static void UpdateDataOwnerRecursively(IDataOwner dataOwner)
        {
            IEnumerable<MemberInfo> properties = EditorReflectionUtils.GetAllDataMembers(dataOwner);

            foreach (MemberInfo property in properties)
            {
                IUpdater updater = GetUpdaterForType(ReflectionUtils.GetDeclaredTypeOfPropertyOrField(property));

                if (updater != null)
                {
                    updater.Update(property, dataOwner);
                }
            }

            IEntityCollectionData entityCollectionData = dataOwner.Data as IEntityCollectionData;
            if (entityCollectionData != null)
            {
                IEnumerable<IDataOwner> childDataOwners = entityCollectionData.GetChildren().Where(child => child is IDataOwner).Cast<IDataOwner>();
                foreach (IDataOwner childDataOwner in entityCollectionData.GetChildren())
                {
                    UpdateDataOwnerRecursively(childDataOwner);
                }
            }
        }

        /// <summary>
        /// Returns a suitable updater for the provided type, or null if none is found.
        /// </summary>
        public static IUpdater GetUpdaterForType(Type type)
        {
            Type currentType = type;
            // Get updater for type, checking from the most concrete type definition to a most abstract one.
            while (currentType.IsInterface == false && currentType != typeof(object))
            {
                IUpdater concreteTypeUpdater = GetUpdater(currentType);
                if (concreteTypeUpdater != null)
                {
                    return concreteTypeUpdater;
                }

                currentType = currentType.BaseType;
            }

            IUpdater interfaceUpdater = null;
            if (type.IsInterface)
            {
                interfaceUpdater = GetUpdater(type);
            }

            if (interfaceUpdater == null)
            {
                interfaceUpdater = GetInheritedInterfaceUpdater(type);
            }

            if (interfaceUpdater != null)
            {
                return interfaceUpdater;
            }

            return null;
        }

        private static IUpdater GetUpdater(Type type)
        {
            return Updaters.FirstOrDefault(updater => updater.UpdatedType == type);
        }

        private static IUpdater GetInheritedInterfaceUpdater(Type type)
        {
            return type.GetInterfaces().Select(GetUpdater).FirstOrDefault(t => t != null);
        }
    }
}
