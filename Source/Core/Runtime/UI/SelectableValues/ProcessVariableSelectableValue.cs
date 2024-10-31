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
        public T Value => IsFirstValueSelected ? FirstValue : SecondValue.Value.GetValue();

        public ProcessVariableSelectableValue(T firstValue, SingleScenePropertyReference<IDataProperty<T>> secondValue, bool isFirstValueSelected = true)
        {
            FirstValue = firstValue;
            SecondValue = secondValue;
            IsFirstValueSelected = isFirstValueSelected;
        }

        public ProcessVariableSelectableValue()
        {
            IsFirstValueSelected = true;
        }
    }
}
