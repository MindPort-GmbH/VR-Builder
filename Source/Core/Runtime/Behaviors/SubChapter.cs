using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Represents a nested chapter that can be optionally skipped.
    /// </summary>
    [DataContract(IsReference = true)]
    public class SubChapter
    {
        /// <summary>
        /// The chapter to execute.
        /// </summary>
        [DataMember]
        public IChapter Chapter { get; }

        /// <summary>
        /// If true, the chapter can be skipped by the user or the process.
        /// </summary>
        [DataMember]
        public bool IsOptional { get; set; }

        public SubChapter(IChapter chapter) : this(chapter, false)
        {
        }

        [JsonConstructor]
        public SubChapter(IChapter chapter, bool isOptional)
        {
            Chapter = chapter;
            IsOptional = isOptional;
        }
    }
}