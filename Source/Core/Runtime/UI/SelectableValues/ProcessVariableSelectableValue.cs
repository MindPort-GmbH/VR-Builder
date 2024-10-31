using System;
using System.Runtime.Serialization;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.UI.SelectableValues
{
    /// <summary>
    /// Selectable value implementation for process variables.
    /// </summary>    
    [DataContract(IsReference = true)]
    public class ProcessVariableSelectableValue<T> : SelectableValue<T, SingleScenePropertyReference<IDataProperty<T>>>
    {
        /// <inheritdoc/>        
        public override string FirstValueLabel => "Constant";

        /// <inheritdoc/>        
        public override string SecondValueLabel => "Data Property";

        /// <summary>
        /// Returns the value from the currently selected property.
        /// </summary>
        public T Value
        {
            get
            {
                if (IsFirstValueSelected)
                {
                    return FirstValue;
                }
                else
                {
                    if (SecondValue.HasValue())
                    {
                        return SecondValue.Value.GetValue();
                    }
                    else
                    {
                        return default(T);
                    }
                }
            }
        }

        public ProcessVariableSelectableValue(T firstValue, SingleScenePropertyReference<IDataProperty<T>> secondValue, bool isFirstValueSelected = true)
        {
            FirstValue = firstValue;
            SecondValue = secondValue;
            IsFirstValueSelected = isFirstValueSelected;
        }

        public ProcessVariableSelectableValue()
        {
            FirstValue = default;
            SecondValue = new SingleScenePropertyReference<IDataProperty<T>>();
            IsFirstValueSelected = true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            ProcessVariableSelectableValue<T> selectableValue = obj as ProcessVariableSelectableValue<T>;

            return GetType() == selectableValue.GetType() &&
                FirstValue.Equals(selectableValue.FirstValue) &&
                SecondValue.Equals(selectableValue.SecondValue) &&
                IsFirstValueSelected.Equals(selectableValue.IsFirstValueSelected);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FirstValue, SecondValue, IsFirstValueSelected);
        }

        public static bool operator ==(ProcessVariableSelectableValue<T> left, ProcessVariableSelectableValue<T> right)
        {
            if ((object)left == null)
            {
                return (object)right == null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(ProcessVariableSelectableValue<T> left, ProcessVariableSelectableValue<T> right)
        {
            if ((object)left == null)
            {
                return (object)right != null;
            }

            return left.Equals(right) == false;
        }
    }
}
