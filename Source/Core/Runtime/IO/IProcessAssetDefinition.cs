using System.Collections.Generic;
using VRBuilder.Core.Serialization;

namespace VRBuilder.Core.IO
{
    /// <summary>
    /// Defines how a process asset should be saved or loaded.
    /// </summary>
    public interface IProcessAssetDefinition
    {
        IEnumerable<string> GetProcessAssetPaths(string path);

        IEnumerable<ProcessAssetData> GetSerializedProcessAssets(IProcess process, IProcessSerializer serializer);

        IProcess GetProcessFromSerializedData(IEnumerable<ProcessAssetData> data, IProcessSerializer serializer);
    }
}
