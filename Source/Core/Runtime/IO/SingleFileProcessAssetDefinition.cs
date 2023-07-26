using System;
using System.Collections.Generic;
using UnityEngine;
using VRBuilder.Core.Serialization;

namespace VRBuilder.Core.IO
{
    public class SingleFileProcessAssetDefinition : IProcessAssetDefinition
    {
        public bool CreateManifest => false;

        public IDictionary<string, byte[]> CreateSerializedProcessAssets(IProcess process, IProcessSerializer serializer)
        {
            return new Dictionary<string, byte[]>
            {
                { process.Data.Name, serializer.ProcessToByteArray(process) }
            };
        }

        public IProcess GetProcessFromSerializedData(byte[] processData, IEnumerable<byte[]> additionalData, IProcessSerializer serializer)
        {
            try
            {
                return serializer.ProcessFromByteArray(processData);
            }
            catch (Exception ex)
            {
                //Debug.LogError($"Failed to load the process '{processName}' from '{processAssetPath}' because of: \n{ex.Message}");
                Debug.LogError(ex);
            }

            return null;
        }
    }
}
