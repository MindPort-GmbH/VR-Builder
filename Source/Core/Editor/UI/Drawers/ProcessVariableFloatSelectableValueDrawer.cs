using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.UI.Drawers
{
    /// <summary>
    /// Concrete implementation of process variable selectable value drawer.
    /// </summary>
    [DefaultProcessDrawer(typeof(ProcessVariableSelectableValue<float>))]
    public class ProcessVariableFloatSelectableValueDrawer : SelectableValueDrawer<float, SingleScenePropertyReference<IDataProperty<float>>>
    {
    }
}
