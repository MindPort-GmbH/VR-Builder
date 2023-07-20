using System.Collections.Generic;
using VRBuilder.Core.Serialization;

namespace VRBuilder.Core.IO
{
    /// <summary>
    /// Defines how a process asset should be saved or loaded.
    /// </summary>
    public interface IProcessAssetDefinition
    {
        IDictionary<string, byte[]> CreateSerializedProcessAssets(IProcess process, IProcessSerializer serializer);

        IProcess GetProcessFromSerializedData(byte[] processData, IEnumerable<byte[]> additionalData, IProcessSerializer serializer);
    }
}
