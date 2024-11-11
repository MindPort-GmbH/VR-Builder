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
    }
}
