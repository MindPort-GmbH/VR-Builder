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

            // TODO Consider passing IDataOwner as ref
            UpdateDataRecursively(process.Data);

            ProcessAssetManager.Save(process);
        }

        private static void UpdateDataRecursively(IData data)
        {
            // For each behavior and condition, check if there is a custom converter for this data type and if so use it to replace it with a new version.

            // If there is no custom converter, apply the default one.
            EntityDataUpdater dataUpdater = new EntityDataUpdater();

            //data = dataUpdater.GetUpdatedData(data);

            IEntityCollectionData entityCollectionData = data as IEntityCollectionData;
            if (entityCollectionData != null)
            {
                IEnumerable<IDataOwner> childDataOwners = entityCollectionData.GetChildren().Where(child => child is IDataOwner).Cast<IDataOwner>();
                foreach (IDataOwner dataOwner in entityCollectionData.GetChildren())
                {
                    UpdateDataRecursively(dataOwner.Data);
                }
            }
        }
    }
}
