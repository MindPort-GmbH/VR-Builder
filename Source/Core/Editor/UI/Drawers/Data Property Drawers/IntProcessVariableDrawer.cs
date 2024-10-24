using UnityEditor;
using VRBuilder.Core.ProcessUtils;
using VRBuilder.Editor.UI.Drawers;

namespace VRBuilder.Editor.Core.UI.Drawers
{
    /// <summary>
    /// Implementation of <see cref="ProcessVariableDrawer{T}"/> that draws int variables.
    /// </summary>
    [DefaultProcessDrawer(typeof(ProcessVariable<int>))]
    internal class IntProcessVariableDrawer : ProcessVariableDrawer<int>
    {
        /// <inheritdoc/>
        protected override int DrawConstField(int value)
        {
            return EditorGUILayout.IntField("", value);
        }
    }
}