using UnityEditor;
using VRBuilder.Core.ProcessUtils;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Implementation of <see cref="ProcessVariableDrawer{T}"/> that draws bool variables.
    /// </summary>
    [DefaultProcessDrawer(typeof(ProcessVariable<bool>))]
    internal class BooleanProcessVariableDrawer : ProcessVariableDrawer<bool>
    {
        /// <inheritdoc/>
        protected override bool DrawConstField(bool value)
        {
            return EditorGUILayout.Toggle(value);
        }
    }
}
