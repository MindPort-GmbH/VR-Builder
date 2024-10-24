using UnityEditor;
using VRBuilder.Core.ProcessUtils;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Implementation of <see cref="ProcessVariableDrawer{T}"/> that draws float variables.
    /// </summary>
    [DefaultProcessDrawer(typeof(ProcessVariable<float>))]
    internal class FloatProcessVariableDrawer : ProcessVariableDrawer<float>
    {
        /// <inheritdoc/>
        protected override float DrawConstField(float value)
        {
            return EditorGUILayout.FloatField("", value);
        }
    }
}
