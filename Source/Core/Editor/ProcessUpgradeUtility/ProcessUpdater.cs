using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private static IEnumerable<IUpdater> updaters;
        private static int totalSteps;
        private static int progress;

        public static IEnumerable<IUpdater> Updaters
        {
            get
            {
                if (updaters == null)
                {
                    updaters = ReflectionUtils.GetConcreteImplementationsOf<IUpdater>()
                        .Select(type => ReflectionUtils.CreateInstanceOfType(type))
                        .Cast<IUpdater>();
                }

                return updaters;
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

            progress = 0;

            foreach (IChapter chapter in process.Data.Chapters)
            {
                totalSteps += chapter.Data.Steps.Count;
            }

            UpdateDataRecursively(process);


            ProcessAssetManager.Save(process);

            EditorUtility.ClearProgressBar();

        }

        public static void UpdateDataRecursively(IDataOwner dataOwner)
        {
            if (dataOwner is IStep)
            {
                progress++;
                EditorUtility.DisplayProgressBar("Updating process", ((IStep)dataOwner).Data.Name, (float)progress / totalSteps);
            }

            IEnumerable<MemberInfo> properties = EditorReflectionUtils.GetAllFieldsAndProperties(dataOwner).Where(memberInfo => memberInfo.MemberType == MemberTypes.Property);

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

        //public static IPropertyUpdater GetPropertyUpdater(Type type)
        //{
        //    Type currentType = type;
        //    // Get updater for type, checking from the most concrete type definition to a most abstract one.
        //    while (currentType.IsInterface == false && currentType != typeof(object))
        //    {
        //        IPropertyUpdater concreteTypeUpdater = GetTypePropertyUpdater(currentType);
        //        if (concreteTypeUpdater != null)
        //        {
        //            return concreteTypeUpdater;
        //        }

        //        currentType = currentType.BaseType;
        //    }

        //    IPropertyUpdater interfaceUpdater = null;
        //    if (type.IsInterface)
        //    {
        //        interfaceUpdater = GetTypePropertyUpdater(type);
        //    }

        //    if (interfaceUpdater == null)
        //    {
        //        interfaceUpdater = type.GetInterfaces().Select(GetTypePropertyUpdater).FirstOrDefault(t => t != null);
        //    }

        //    if (interfaceUpdater != null)
        //    {
        //        return interfaceUpdater;
        //    }

        //    return null;
        //}

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

        //private static IPropertyUpdater GetTypePropertyUpdater(Type type)
        //{
        //    return PropertyUpdaters.FirstOrDefault(updater => updater.SupportedType == type);
        //}

        private static IUpdater GetUpdater(Type type)
        {
            return Updaters.FirstOrDefault(updater => updater.SupportedType == type);
        }

        private static IUpdater GetInheritedInterfaceUpdater(Type type)
        {
            return type.GetInterfaces().Select(GetUpdater).FirstOrDefault(t => t != null);
        }
        //private static IUpdater GetBaseUpdater()
        //{
        //    return GetUpdater(typeof(IDataOwner));
        //}
    }
}
