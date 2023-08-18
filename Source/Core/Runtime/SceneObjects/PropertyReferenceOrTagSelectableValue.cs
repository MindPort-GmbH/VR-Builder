using System.Runtime.Serialization;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.SceneObjects
{
    [DataContract]
    public class PropertyReferenceOrTagSelectableValue<T> : SelectableValue<ScenePropertyReference<T>, SceneObjectTag<T>> where T : class, ISceneObjectProperty
    {
        public override string FirstValueLabel => "Property Reference";

        public override string SecondValueLabel => "Tag";

        public PropertyReferenceOrTagSelectableValue()
        {
            FirstValue = new ScenePropertyReference<T>();
            SecondValue = new SceneObjectTag<T>();
        }
    }
}
