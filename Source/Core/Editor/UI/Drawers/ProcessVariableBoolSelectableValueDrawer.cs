using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.UI.Drawers
{
    [DefaultProcessDrawer(typeof(ProcessVariableSelectableValue<bool>))]
    public class ProcessVariableBoolSelectableValueDrawer : SelectableValueDrawer<bool, SingleScenePropertyReference<IDataProperty<bool>>>
    {
    }
}
