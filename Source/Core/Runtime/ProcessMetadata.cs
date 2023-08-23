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
        /// String localization table for this process.
        /// </summary>
        [DataMember]
        public string StringLocalizationTable { get; set; }

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
