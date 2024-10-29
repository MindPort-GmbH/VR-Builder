using System.Runtime.Serialization;

namespace VRBuilder.Core.UI.SelectableValues
{
    /// <summary>
    /// Stores two values and a selection between the two.
    /// </summary>
    [DataContract(IsReference = true)]
    public abstract class SelectableValue<TFirst, TSecond>
    {
        /// <summary>
        /// Label for the first value.
        /// </summary>
        public abstract string FirstValueLabel { get; }

        /// <summary>
        /// Label for the second value.
        /// </summary>
        public abstract string SecondValueLabel { get; }

        /// <summary>
        /// The first value to be stored.
        /// </summary>
        [DataMember]
        public virtual TFirst FirstValue { get; set; }

        /// <summary>
        /// The second value to be stored.
        /// </summary>
        [DataMember]
        public virtual TSecond SecondValue { get; set; }

        /// <summary>
        /// True if the first value should be used.
        /// </summary>
        [DataMember]
        public virtual bool IsFirstValueSelected { get; set; }
    }
}
