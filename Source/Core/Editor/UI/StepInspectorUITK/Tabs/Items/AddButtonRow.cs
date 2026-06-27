using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items
{
    /// <summary>
    /// Lays out an "Add …" button with a paste icon button beside it.
    /// The paste button is only enabled while the system clipboard holds an entity that
    /// matches the target list (checked via <paramref name="canPaste"/>); clicking it
    /// appends that entity to the list. The enabled state is kept in sync with the clipboard
    /// by polling, since the clipboard can change while the inspector is open.
    /// </summary>
    internal static class AddButtonRow
    {
        private const long ClipboardPollIntervalMs = 250;

        // Same icon the legacy Step Inspector uses for its paste action (icon_paste_light/dark).
        private static readonly EditorIcon pasteIcon = new EditorIcon("icon_paste");

        public static VisualElement Build(Button addButton, Func<bool> canPaste, Action onPaste, string pasteTooltip)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("vrb-add-row");

            Button pasteButton = new Button(() =>
            {
                if (canPaste() == false) return;
                onPaste();
            })
            {
                tooltip = pasteTooltip
            };
            pasteButton.AddToClassList("vrb-paste-button");

            // Icon lives as a child Image so it's easy to restyle / swap independently of the button.
            Image icon = new Image
            {
                image = pasteIcon.Texture,
                scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore
            };
            icon.AddToClassList("vrb-paste-button__icon");
            pasteButton.Add(icon);

            void Refresh() => pasteButton.SetEnabled(canPaste());
            Refresh();
            pasteButton.schedule.Execute(Refresh).Every(ClipboardPollIntervalMs);

            row.Add(addButton);
            row.Add(pasteButton);
            return row;
        }
    }
}
