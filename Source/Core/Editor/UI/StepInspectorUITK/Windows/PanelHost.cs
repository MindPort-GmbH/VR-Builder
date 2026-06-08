using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    /// <summary>
    /// Header + body container shared by every panel window so panels look identical
    /// regardless of which window is hosting them.
    /// </summary>
    internal sealed class PanelHost : VisualElement
    {
        public string PanelId { get; }
        public VisualElement Header { get; }

        public PanelHost(string panelId, string title, VisualElement body, Texture icon = null)
        {
            PanelId = panelId;
            AddToClassList("vrb-panel-host");

            VisualElement header = new VisualElement { name = "vrb-panel-host__header" };
            header.AddToClassList("vrb-panel-host__header");
            Header = header;

            if (icon != null)
            {
                Image iconImage = new Image { name = "vrb-panel-host__icon", image = icon };
                iconImage.AddToClassList("vrb-panel-host__icon");
                header.Add(iconImage);
            }

            Label titleLabel = new Label(title ?? string.Empty) { name = "vrb-panel-host__title" };
            titleLabel.AddToClassList("vrb-panel-host__title");
            header.Add(titleLabel);

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
