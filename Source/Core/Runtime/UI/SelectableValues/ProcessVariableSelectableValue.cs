using System.Runtime.Serialization;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Selectable value implementation for process variables.
    /// </summary>    
    [DataContract(IsReference = true)]
    public class ProcessVariableSelectableValue<T> : SelectableValue<T, SingleScenePropertyReference<IDataProperty<T>>>
    {
        public override string FirstValueLabel => "Constant";

        public override string SecondValueLabel => "Data Property";

        public ProcessVariableSelectableValue(T firstValue, SingleScenePropertyReference<IDataProperty<T>> secondValue, bool isFirstValueSelected)
        {
            FirstValue = firstValue;
            SecondValue = secondValue;
            IsFirstValueSelected = isFirstValueSelected;
        }
    }
}
