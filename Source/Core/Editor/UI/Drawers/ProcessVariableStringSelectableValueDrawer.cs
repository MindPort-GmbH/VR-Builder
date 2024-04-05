using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.UI.Drawers
{
    [DefaultProcessDrawer(typeof(ProcessVariableSelectableValue<string>))]
    public class ProcessVariableStringSelectableValueDrawer : SelectableValueDrawer<string, SingleScenePropertyReference<IDataProperty<string>>>
    {
    }
}
