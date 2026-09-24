// Copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI;
using VRBuilder.Core.Editor.Utils;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Elements
{
    /// <summary>
    /// Reusable UIToolkit element that renders a foldable header bar with action buttons
    /// (delete, menu, reorder), matching the IMGUI MetadataWrapperDrawer visual style.
    /// Uses a triangle arrow (▶/▼) for fold indicator instead of a checkmark toggle.
    /// </summary>
    public class FoldableItemElement : VisualElement
    {
        private static readonly EditorIcon deleteIcon = new EditorIcon("icon_delete");
        private static readonly EditorIcon arrowUpIcon = new EditorIcon("icon_arrow_up");
        private static readonly EditorIcon arrowDownIcon = new EditorIcon("icon_arrow_down");
        private static readonly EditorIcon menuIcon = new EditorIcon("icon_menu");

        private readonly Label foldoutArrow;
        private readonly Label displayNameLabel;
        private readonly Button menuButton;
        private readonly Button deleteButton;
        private readonly Button moveUpButton;
        private readonly Button moveDownButton;
        private readonly VisualElement contentContainer;
        private readonly VisualElement headerContainer;

        private bool isFolded;

        private const string ArrowRight = "\u25B6"; // ▶ (folded / collapsed)
        private const string ArrowDown = "\u25BC";  // ▼ (expanded / shown)

        /// <summary>
        /// The content container where drawer-generated content is placed.
        /// </summary>
        public VisualElement Content => contentContainer;

        /// <summary>
        /// Whether the item is currently folded (collapsed).
        /// </summary>
        public bool IsFolded
        {
            get => isFolded;
            set
            {
                if (isFolded == value) return;
                isFolded = value;
                contentContainer.style.display = isFolded ? DisplayStyle.None : DisplayStyle.Flex;
                foldoutArrow.text = isFolded ? ArrowRight : ArrowDown;
                OnFoldStateChanged?.Invoke(isFolded);
            }
        }

        /// <summary>
        /// Callback invoked when fold state changes. Parameter is the new isFolded value.
        /// </summary>
        public Action<bool> OnFoldStateChanged { get; set; }

        /// <summary>
        /// Callback invoked when the delete button is clicked.
        /// </summary>
        public Action OnDelete { get; set; }

        /// <summary>
        /// Callback invoked when the move up button is clicked.
        /// </summary>
        public Action OnMoveUp { get; set; }

        /// <summary>
        /// Callback invoked when the move down button is clicked.
        /// </summary>
        public Action OnMoveDown { get; set; }

        /// <summary>
        /// Callback invoked to copy the entity.
        /// </summary>
        public Action OnCopy { get; set; }

        /// <summary>
        /// Callback invoked to paste an entity.
        /// </summary>
        public Action OnPaste { get; set; }

        /// <summary>
        /// Callback invoked to remove via menu.
        /// </summary>
        public Action OnRemove { get; set; }

        /// <summary>
        /// Creates a new FoldableItemElement.
        /// </summary>
        /// <param name="displayName">Display name shown in the header.</param>
        /// <param name="startFolded">Whether the item starts folded.</param>
        /// <param name="isFirst">Whether this is the first item in the list.</param>
        /// <param name="isLast">Whether this is the last item in the list.</param>
        public FoldableItemElement(string displayName, bool startFolded = true, bool isFirst = false, bool isLast = false)
        {
            // Header container - gray bar matching MetadataWrapperDrawer.DrawFoldable
            headerContainer = new VisualElement();
            headerContainer.AddToClassList("foldable-item__header");

            // Triangle arrow label (▶ / ▼) — replaces Toggle checkmark
            foldoutArrow = new Label(startFolded ? ArrowRight : ArrowDown);
            foldoutArrow.AddToClassList("foldable-item__foldout-arrow");
            headerContainer.Add(foldoutArrow);

            // Toggle fold on click anywhere on the header EXCEPT action buttons.
            headerContainer.RegisterCallback<PointerUpEvent>(evt =>
            {
                VisualElement target = evt.target as VisualElement;

                // Walk up from the target to check if it's inside an action button
                VisualElement walk = target;
                while (walk != null && walk != headerContainer)
                {
                    if (walk is Button) return; // Skip — button has its own handler
                    walk = walk.parent;
                }

                // Clicked on header area (arrow, label, spacer, or header bg)
                IsFolded = !IsFolded;
                evt.StopPropagation();
            });

            // Display name label
            displayNameLabel = new Label(displayName);
            displayNameLabel.AddToClassList("foldable-item__header-label");
            headerContainer.Add(displayNameLabel);

            // Flexible spacer
            VisualElement spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            headerContainer.Add(spacer);

            // Menu button (three dots)
            menuButton = CreateIconButton(menuIcon, "Menu", OnMenuClicked);
            menuButton.AddToClassList("foldable-item__header-button");
            headerContainer.Add(menuButton);

            // Delete button
            deleteButton = CreateIconButton(deleteIcon, "Delete", () => OnDelete?.Invoke());
            deleteButton.AddToClassList("foldable-item__header-button");
            headerContainer.Add(deleteButton);

            // Move up button
            moveUpButton = CreateIconButton(arrowUpIcon, "Move Up", () => OnMoveUp?.Invoke());
            moveUpButton.AddToClassList("foldable-item__header-button");
            moveUpButton.SetEnabled(!isFirst);
            headerContainer.Add(moveUpButton);

            // Move down button
            moveDownButton = CreateIconButton(arrowDownIcon, "Move Down", () => OnMoveDown?.Invoke());
            moveDownButton.AddToClassList("foldable-item__header-button");
            moveDownButton.SetEnabled(!isLast);
            headerContainer.Add(moveDownButton);

            hierarchy.Add(headerContainer);

            // Content container - indented, shown/hidden by foldout
            contentContainer = new VisualElement();
            contentContainer.AddToClassList("foldable-item__content");
            hierarchy.Add(contentContainer);

            // Apply initial fold state (set directly, don't invoke callback)
            isFolded = startFolded;
            contentContainer.style.display = isFolded ? DisplayStyle.None : DisplayStyle.Flex;
        }

        /// <summary>
        /// Updates the display name shown in the header.
        /// </summary>
        public void SetDisplayName(string name)
        {
            displayNameLabel.text = name;
        }

        /// <summary>
        /// Updates the reorder button states based on position in list.
        /// </summary>
        public void SetPosition(bool isFirst, bool isLast)
        {
            moveUpButton.SetEnabled(!isFirst);
            moveDownButton.SetEnabled(!isLast);
        }

        private void OnMenuClicked()
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Remove"), false, () => OnRemove?.Invoke());
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Copy"), false, () => OnCopy?.Invoke());

            if (OnPaste != null)
            {
                menu.AddItem(new GUIContent("Paste"), false, () => OnPaste?.Invoke());
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Paste"));
            }

            menu.ShowAsContext();
        }

        private static Button CreateIconButton(EditorIcon icon, string tooltip, Action onClick)
        {
            Button button = new Button(() => onClick?.Invoke());
            button.tooltip = tooltip;

            try
            {
                Texture iconTexture = icon.Texture;
                if (iconTexture != null)
                {
                    Image iconImage = new Image { image = iconTexture };
                    iconImage.AddToClassList("foldable-item__button-icon");
                    button.Add(iconImage);
                }
                else
                {
                    button.text = tooltip.Substring(0, 1);
                }
            }
            catch
            {
                // If icon loading fails, use text fallback
                button.text = tooltip.Substring(0, 1);
            }

            return button;
        }

        /// <summary>
        /// Creates a horizontal separator line matching the IMGUI SeparatedAttribute style.
        /// </summary>
        public static VisualElement CreateSeparator()
        {
            VisualElement separator = new VisualElement();
            separator.AddToClassList("foldable-item__separator");
            return separator;
        }

        /// <summary>
        /// Creates a centered "Add" button matching the IMGUI EditorDrawingHelper.DrawAddButton style.
        /// </summary>
        public static Button CreateAddButton(string label, Action onClick)
        {
            Button button = new Button(() => onClick?.Invoke());
            button.text = label;
            button.AddToClassList("step-inspector__add-button");
            return button;
        }

        /// <summary>
        /// Creates a small paste button.
        /// </summary>
        public static Button CreatePasteButton(Action onClick)
        {
            Button button = new Button(() => onClick?.Invoke());
            button.text = "Paste";
            button.tooltip = "Paste from clipboard";
            button.AddToClassList("step-inspector__paste-button");
            return button;
        }
    }
}
