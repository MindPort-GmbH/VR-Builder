using System;
using System.Runtime.Serialization;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Lets the user choose between a scene property reference or a tag.
    /// </summary>
    [DataContract(IsReference = true)]
    [Obsolete("This class is not used anymore and will be removed in the next major release.")]
    public class PropertyReferenceOrTagSelectableValue<T> : SelectableValue<ScenePropertyReference<T>, SceneObjectTag<T>> where T : class, ISceneObjectProperty
    {
        /// <inheritdoc/>
        public override string FirstValueLabel => "Property Reference";

        /// <inheritdoc/>
        public override string SecondValueLabel => "Tag";

        public PropertyReferenceOrTagSelectableValue()
        {
            IsFirstValueSelected = true;
            FirstValue = new ScenePropertyReference<T>();
            SecondValue = new SceneObjectTag<T>();
        }
    }
}
