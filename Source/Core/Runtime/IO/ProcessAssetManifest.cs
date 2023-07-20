using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VRBuilder.Core.IO
{
    [DataContract(IsReference = true)]
    public class ProcessAssetManifest : IProcessAssetManifest
    {
        [DataMember]
        public string AssetDefinition { get; set; }

        [DataMember]
        public List<string> AdditionalFileNames { get; set; }
    }
}
