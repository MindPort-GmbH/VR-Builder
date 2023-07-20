using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Serialization;

namespace VRBuilder.Core.IO
{
    public class SingleFileProcessAssetDefinition : IProcessAssetDefinition
    {
        public IEnumerable<string> GetProcessAssetPaths(string path)
        {
            return new[] { path };
        }

        public IProcess GetProcessFromSerializedData(IEnumerable<ProcessAssetData> data, IProcessSerializer serializer)
        {
            if(data.Count() != 1)
            {
                Debug.LogError("Multiple data found for single file process.");
            }

            try
            {
                return serializer.ProcessFromByteArray(data.First().Data);
            }
            catch (Exception ex)
            {
                //Debug.LogError($"Failed to load the process '{processName}' from '{processAssetPath}' because of: \n{ex.Message}");
                Debug.LogError(ex);
            }

            return null;
        }

        public IEnumerable<ProcessAssetData> GetSerializedProcessAssets(IProcess process, IProcessSerializer serializer)
        {
            return new[] { new ProcessAssetData(serializer.ProcessToByteArray(process), process.Data.Name) };
        }
    }
}
