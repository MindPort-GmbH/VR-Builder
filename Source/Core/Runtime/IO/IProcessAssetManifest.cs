using System.Collections.Generic;

namespace VRBuilder.Core.IO
{
    public interface IProcessAssetManifest
    {
        string AssetDefinition { get; set; }

        string ProcessFileName { get; set; }

        IEnumerable<string> AdditionalFileNames { get; set; }
    }
}
