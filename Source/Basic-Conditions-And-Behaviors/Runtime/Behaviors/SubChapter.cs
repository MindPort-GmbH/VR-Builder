using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace VRBuilder.Core.Behaviors
{
    [DataContract(IsReference = true)]
    public class SubChapter
    {
        [DataMember]
        public IChapter Chapter { get; }

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