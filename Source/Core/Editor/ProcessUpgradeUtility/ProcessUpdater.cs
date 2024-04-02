using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.EntityOwners;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;
using VRBuilder.Unity;

namespace VRBuilder.Editor.ProcessUpdater
{
    public static class ProcessUpdater
    {
        private static IEnumerable<IUpdater> updaters;
        private static IEnumerable<IEntityConverter> entityConverters;

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

        public static IEnumerable<IEntityConverter> EntityConverters
        {
            get
            {
                if (entityConverters == null)
                {
                    entityConverters = new List<IEntityConverter>(ReflectionUtils.GetConcreteImplementationsOf<IEntityConverter>()
                        .Select(ReflectionUtils.CreateInstanceOfType)
                        .Cast<IEntityConverter>());
                }

                return entityConverters;
            }
        }

        [MenuItem("Tools/VR Builder/Developer/Update Process in Scene", false, 70)]
        private static void UpdateProcessMenuEntry()
        {
            if (RuntimeConfigurator.Exists == false)
            {
                Debug.LogError("This is not a VR Builder scene");
                return;
            }

            IProcess process = GlobalEditorHandler.GetCurrentProcess();

            if (process == null)
            {
                Debug.LogError("No active process found.");
                return;
            }

            IEnumerable<ProcessSceneObject> processSceneObjects = SceneUtils.GetActiveAndInactiveComponents<ProcessSceneObject>();
            foreach (ProcessSceneObject sceneObject in processSceneObjects)
            {
                sceneObject.ResetUniqueId();
            }

            UpdateDataRecursively(process);

            ProcessAssetManager.Save(process);
        }

        public static void UpdateDataRecursively(IDataOwner dataOwner)
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
                    UpdateDataRecursively(childDataOwner);
                }
            }
        }

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
            return Updaters.FirstOrDefault(updater => updater.SupportedType == type);
        }

        private static IUpdater GetInheritedInterfaceUpdater(Type type)
        {
            return type.GetInterfaces().Select(GetUpdater).FirstOrDefault(t => t != null);
        }
    }
}
