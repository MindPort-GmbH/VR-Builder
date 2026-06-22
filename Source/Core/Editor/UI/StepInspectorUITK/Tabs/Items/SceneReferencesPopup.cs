using System;
using UnityEditor;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI.GraphView.Windows;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items
{
    /// <summary>
    /// Opens the searchable Scene References popup (<see cref="SceneReferencesEditorPopup"/>)
    /// anchored under a UITK control. Used when a reference resolves to multiple objects or groups.
    /// </summary>
    internal static class SceneReferencesPopup
    {
        public static void Show(
            VisualElement activator,
            ProcessSceneReferenceBase reference,
            Action<object> changeValueCallback)
        {
            SceneReferencesEditorPopup content = new SceneReferencesEditorPopup(reference, changeValueCallback);
            content.SetWindowSize(windowWith: activator.resolvedStyle.width);

            UnityEditor.PopupWindow.Show(activator.worldBound, content);
        }
    }
}
