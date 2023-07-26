using System.Collections.Generic;

namespace VRBuilder.Core.IO
{
    /// <summary>
    /// Provides instructions on how a process asset should be loaded.
    /// </summary>
    public interface IProcessAssetManifest
    {
        /// <summary>
        /// Full type name of the asset strategy used to load the process. It should be a type of <see cref="IProcessAssetStrategy"/>.
        /// </summary>
        string AssetStrategyTypeName { get; set; }

        /// <summary>
        /// Name of the main process file.
        /// </summary>
        string ProcessFileName { get; set; }

        /// <summary>
        /// Names of files containing additional data.
        /// </summary>
        IEnumerable<string> AdditionalFileNames { get; set; }
    }
}
