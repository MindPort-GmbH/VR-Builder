using System.Collections.Generic;
using VRBuilder.Core.Serialization;

namespace VRBuilder.Core.IO
{
    /// <summary>
    /// Defines how a process asset should be saved or loaded.
    /// </summary>
    public interface IProcessAssetStrategy
    {
        /// <summary>
        /// If true, a manifest file will be created. The file will be read first when loading the process.
        /// </summary>
        bool CreateManifest { get; }

        /// <summary>
        /// Returns a number of named byte arrays containing the process data. The first key is the process file,
        /// subsequent ones are additional data.
        /// </summary>
        IDictionary<string, byte[]> CreateSerializedProcessAssets(IProcess process, IProcessSerializer serializer);

        /// <summary>
        /// Attempts to build a process from the provided serialized data.
        /// </summary>
        IProcess GetProcessFromSerializedData(byte[] processData, IEnumerable<byte[]> additionalData, IProcessSerializer serializer);
    }
}
