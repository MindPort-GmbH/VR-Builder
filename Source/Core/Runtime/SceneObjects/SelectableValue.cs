using System.Runtime.Serialization;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract(IsReference = true)]
    public abstract class SelectableValue<TFirst, TSecond>
    {
        public string FirstValueLabel { get; }

        public string SecondValueLabel { get; }

        [DataMember]
        public TFirst FirstValue { get; set; }

        [DataMember]
        public TSecond SecondValue { get; set; }

        [DataMember]
        public bool IsFirstValueSelected { get; set; }
    }
}
