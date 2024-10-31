using System;
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

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SelectableValue<TFirst, TSecond> selectableValue = obj as SelectableValue<TFirst, TSecond>;

            return GetType() == selectableValue.GetType() &&
                FirstValue.Equals(selectableValue.FirstValue) &&
                SecondValue.Equals(selectableValue.SecondValue) &&
                IsFirstValueSelected.Equals(selectableValue.IsFirstValueSelected);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FirstValue, SecondValue, IsFirstValueSelected);
        }

        public static bool operator ==(SelectableValue<TFirst, TSecond> left, SelectableValue<TFirst, TSecond> right)
        {
            if ((object)left == null)
            {
                return (object)right == null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(SelectableValue<TFirst, TSecond> left, SelectableValue<TFirst, TSecond> right)
        {
            if ((object)left == null)
            {
                return (object)right != null;
            }

            return left.Equals(right) == false;
        }
    }
}
