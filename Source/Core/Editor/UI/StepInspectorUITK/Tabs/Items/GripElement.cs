using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items
{
    /// <summary>
    /// Drag-handle shown at the left of behavior / transition / condition rows.
    /// Uses the shared <see cref="EditorIcon"/> grip PNGs (same pattern as paste in
    /// <see cref="AddButtonRow"/>).
    /// </summary>
    internal static class GripElement
    {
        private static readonly EditorIcon GripIcon = new EditorIcon("icon_grip");

        public static VisualElement Create(string tooltip)
        {
            VisualElement grip = new VisualElement();
            grip.AddToClassList("vrb-grip");
            grip.tooltip = tooltip;

            Image icon = new Image
            {
                image = GripIcon.Texture,
                scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore
            };
            icon.AddToClassList("vrb-grip__icon");
            grip.Add(icon);

            return grip;
        }
    }
}
