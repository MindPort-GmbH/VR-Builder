using System;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    /// <summary>
    /// Header + body container shared by every panel window so panels look identical
    /// regardless of which window is hosting them. The header carries a "duplicate"
    /// button when an <paramref name="onDuplicate"/> callback is supplied — clicking
    /// it spawns another window of the same panel id, side-by-side with this one.
    /// </summary>
    internal sealed class PanelHost : VisualElement
    {
        public string PanelId { get; }
        public VisualElement Header { get; }

        public PanelHost(string panelId, string title, VisualElement body, Action onDuplicate = null)
        {
            PanelId = panelId;
            AddToClassList("vrb-panel-host");

            VisualElement header = new VisualElement { name = "vrb-panel-host__header" };
            header.AddToClassList("vrb-panel-host__header");
            Header = header;

            Label titleLabel = new Label(title ?? string.Empty) { name = "vrb-panel-host__title" };
            titleLabel.AddToClassList("vrb-panel-host__title");
            header.Add(titleLabel);

            if (onDuplicate != null)
            {
                Button duplicate = new Button(() => onDuplicate())
                {
                    text = "+",
                    tooltip = "Open another " + (title ?? "panel") + " window"
                };
                duplicate.AddToClassList("vrb-panel-host__duplicate");
                header.Add(duplicate);
            }

            VisualElement bodyContainer = new VisualElement { name = "vrb-panel-host__body" };
            bodyContainer.AddToClassList("vrb-panel-host__body");
            if (body != null)
            {
                bodyContainer.Add(body);
            }

            Add(header);
            Add(bodyContainer);
        }
    }
}
