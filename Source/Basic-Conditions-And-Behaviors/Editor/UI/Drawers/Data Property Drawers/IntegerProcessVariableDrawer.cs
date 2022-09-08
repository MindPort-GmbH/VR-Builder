using UnityEditor;
using VRBuilder.Core.ProcessUtils;
using VRBuilder.Editor.UI.Drawers;

namespace VRBuilder.Editor.Core.UI.Drawers
{
    /// <summary>
    /// Implementation of <see cref="ProcessVariableDrawer{T}"/> that draws float variables.
    /// </summary>
    [DefaultProcessDrawer(typeof(ProcessVariable<int>))]
    internal class IntegerProcessVariableDrawer : ProcessVariableDrawer<int>
    {
        /// <inheritdoc/>
        protected override int DrawConstField(int value)
        {
            return EditorGUILayout.IntField("", value);
        }
    }
}