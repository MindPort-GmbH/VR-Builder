using System.Collections.Generic;

namespace VRBuilder.Core.IO
{
    public interface IProcessAssetManifest
    {
        string AssetDefinition { get; set; }
        List<string> AdditionalFileNames { get; set; }
    }
}
