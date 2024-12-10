using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.UI.SelectableValues;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Concrete implementation of process variable selectable value drawer.
    /// </summary>
    [DefaultProcessDrawer(typeof(ProcessVariableSelectableValue<string>))]
    public class ProcessVariableStringSelectableValueDrawer : SelectableValueDrawer<string, SingleScenePropertyReference<IDataProperty<string>>>
    {
    }
}
