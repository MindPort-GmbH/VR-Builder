using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.EntityOwners;

namespace VRBuilder.Editor.Utils
{
    public static class ProcessUpdater
    {
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
            // For each data owner, check if there is a custom converter for this data type and if so use it to replace it with a new version.

            // If there is no custom converter, apply the default one.
            BaseEntityDataUpdater dataUpdater = new BaseEntityDataUpdater();

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

        //private static IProcessDrawer GetDrawerForType(Type type)
        //{
        //    Type currentType = type;
        //    // Get drawer for type, checking from the most concrete type definition to a most abstract one.
        //    while (currentType.IsInterface == false && currentType != typeof(object))
        //    {
        //        IProcessDrawer concreteTypeDrawer = GetTypeDrawer(currentType);
        //        if (concreteTypeDrawer != null)
        //        {
        //            return concreteTypeDrawer;
        //        }

        //        currentType = currentType.BaseType;
        //    }

        //    IProcessDrawer interfaceDrawer = null;
        //    if (type.IsInterface)
        //    {
        //        interfaceDrawer = GetTypeDrawer(type);
        //    }

        //    if (interfaceDrawer == null)
        //    {
        //        interfaceDrawer = GetInheritedInterfaceDrawer(type);
        //    }

        //    if (interfaceDrawer != null)
        //    {
        //        return interfaceDrawer;
        //    }

        //    return GetObjectDrawer();
        //}
    }
}
