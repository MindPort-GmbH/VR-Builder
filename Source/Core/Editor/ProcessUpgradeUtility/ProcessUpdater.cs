using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.EntityOwners;
using VRBuilder.Core.Utils;

namespace VRBuilder.Editor.Utils
{
    public static class ProcessUpdater
    {
        private static IEnumerable<IEntityDataUpdater> entityDataUpdaters;
        private static IEnumerable<IPropertyUpdater> propertyUpdaters;

        public static IEnumerable<IEntityDataUpdater> EntityDataUpdaters
        {
            get
            {
                if (entityDataUpdaters == null)
                {
                    entityDataUpdaters = ReflectionUtils.GetConcreteImplementationsOf<IEntityDataUpdater>()
                        .Select(type => ReflectionUtils.CreateInstanceOfType(type))
                        .Cast<IEntityDataUpdater>();
                }

                return entityDataUpdaters;
            }
        }

        public static IEnumerable<IPropertyUpdater> PropertyUpdaters
        {
            get
            {
                if (propertyUpdaters == null)
                {
                    propertyUpdaters = ReflectionUtils.GetConcreteImplementationsOf<IPropertyUpdater>()
                        .Select(type => ReflectionUtils.CreateInstanceOfType(type))
                        .Cast<IPropertyUpdater>();
                }

                return propertyUpdaters;
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

            UpdateDataRecursively(process);

            ProcessAssetManager.Save(process);
        }

        public static void UpdateDataRecursively(IDataOwner dataOwner)
        {
            IEntityDataUpdater dataUpdater = GetUpdaterForType(dataOwner.Data.GetType());

            dataUpdater.UpdateData(dataOwner);

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

        public static IPropertyUpdater GetPropertyUpdater(Type type)
        {
            Type currentType = type;
            // Get updater for type, checking from the most concrete type definition to a most abstract one.
            while (currentType.IsInterface == false && currentType != typeof(object))
            {
                IPropertyUpdater concreteTypeUpdater = GetTypePropertyUpdater(currentType);
                if (concreteTypeUpdater != null)
                {
                    return concreteTypeUpdater;
                }

                currentType = currentType.BaseType;
            }

            IPropertyUpdater interfaceUpdater = null;
            if (type.IsInterface)
            {
                interfaceUpdater = GetTypePropertyUpdater(type);
            }

            if (interfaceUpdater == null)
            {
                interfaceUpdater = type.GetInterfaces().Select(GetTypePropertyUpdater).FirstOrDefault(t => t != null);
            }

            if (interfaceUpdater != null)
            {
                return interfaceUpdater;
            }

            return null;
        }

        private static IEntityDataUpdater GetUpdaterForType(Type type)
        {
            Type currentType = type;
            // Get updater for type, checking from the most concrete type definition to a most abstract one.
            while (currentType.IsInterface == false && currentType != typeof(object))
            {
                IEntityDataUpdater concreteTypeUpdater = GetUpdater(currentType);
                if (concreteTypeUpdater != null)
                {
                    return concreteTypeUpdater;
                }

                currentType = currentType.BaseType;
            }

            IEntityDataUpdater interfaceUpdater = null;
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

            return GetBaseUpdater();
        }

        private static IPropertyUpdater GetTypePropertyUpdater(Type type)
        {
            return PropertyUpdaters.FirstOrDefault(updater => updater.SupportedType == type);
        }

        private static IEntityDataUpdater GetUpdater(Type type)
        {
            return EntityDataUpdaters.FirstOrDefault(updater => updater.SupportedType == type);
        }

        private static IEntityDataUpdater GetInheritedInterfaceUpdater(Type type)
        {
            return type.GetInterfaces().Select(GetUpdater).FirstOrDefault(t => t != null);
        }
        private static IEntityDataUpdater GetBaseUpdater()
        {
            return GetUpdater(typeof(IDataOwner));
        }
    }
}
