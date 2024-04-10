using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.UI.Drawers
{
    /// <summary>
    /// Concrete implementation of process variable selectable value drawer.
    /// </summary>
    [DefaultProcessDrawer(typeof(ProcessVariableSelectableValue<bool>))]
    public class ProcessVariableBoolSelectableValueDrawer : SelectableValueDrawer<bool, SingleScenePropertyReference<IDataProperty<bool>>>
    {
    }
}
