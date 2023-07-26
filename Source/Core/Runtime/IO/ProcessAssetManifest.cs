using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VRBuilder.Core.IO
{
    /// <summary>
    /// Provides instructions on how a process asset should be loaded.
    /// </summary>
    [Serializable]
    public class ProcessAssetManifest : IProcessAssetManifest
    {
        /// <inheritdoc/>
        [DataMember]
        public string AssetStrategyTypeName { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string ProcessFileName { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public IEnumerable<string> AdditionalFileNames { get; set; }
    }
}
