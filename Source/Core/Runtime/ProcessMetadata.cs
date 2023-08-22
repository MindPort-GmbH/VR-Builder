using System;
using System.Runtime.Serialization;

namespace VRBuilder.Core
{
    /// <summary>
    /// Implementation of <see cref="IMetadata"/> adapted for <see cref="IProcess"/> data.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ProcessMetadata : IMetadata
    {
        /// <summary>
        /// Reference to last selected <see cref="IStep"/>.
        /// </summary>
        [DataMember]
        public string StringLocalizationTable { get; set; }

        /// <summary>
        /// Reference to the entry node's position in the Workflow window.
        /// </summary>
        [DataMember]
        public string AssetLocalizationTable { get; set; }

        /// <summary>
        /// Unique identifier for process.
        /// </summary>
        [DataMember]
        public Guid Guid { get; set; }

        public ProcessMetadata()
        {
        }
    }
}
