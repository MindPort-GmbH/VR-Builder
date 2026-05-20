using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Editor.Utils;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items
{
    /// <summary>
    /// Standard header actions shared by every behavior / condition / transition row.
    /// Returns the buttons in legacy order: Menu (Copy / Paste / Remove) on the left,
    /// optional Help (only when the entity's data type carries <see cref="HelpLinkAttribute"/>)
    /// on the right, just before the trailing Delete button rendered by <see cref="CollapsibleItem"/>.
    /// </summary>
    internal static class EntityHeaderActions
    {
        /// <summary>
        /// Build the standard Menu + Help actions for an <see cref="IEntity"/>.
        /// <paramref name="onRemoved"/> is invoked when the user picks "Remove" from the menu —
        /// callers should mutate the parent list inside that callback.
        /// </summary>
        public static IEnumerable<CollapsibleItem.HeaderAction> BuildStandard(IEntity entity, Action onRemoved)
        {
            yield return BuildMenuAction(entity, onRemoved);

            CollapsibleItem.HeaderAction helpAction = BuildHelpAction(entity);
            if (helpAction.Visible)
            {
                yield return helpAction;
            }
        }

        private static CollapsibleItem.HeaderAction BuildMenuAction(IEntity entity, Action onRemoved)
        {
            return new CollapsibleItem.HeaderAction(
                glyph: Icons.Menu,
                tooltip: "Menu — copy, paste, remove",
                cssModifier: "vrb-item__action--menu",
                callback: () => ShowEntityMenu(entity, onRemoved));
        }

        private static CollapsibleItem.HeaderAction BuildHelpAction(IEntity entity)
        {
            HelpLinkAttribute helpLink = GetHelpLinkAttribute(entity);
            if (helpLink == null || string.IsNullOrEmpty(helpLink.HelpLink))
            {
                return new CollapsibleItem.HeaderAction(Icons.Help, null, null, null, visible: false);
            }

            string url = helpLink.HelpLink;
            return new CollapsibleItem.HeaderAction(
                glyph: Icons.Help,
                tooltip: "Open help documentation: " + url,
                cssModifier: "vrb-item__action--help",
                callback: () => Application.OpenURL(url));
        }

        private static void ShowEntityMenu(IEntity entity, Action onRemoved)
        {
            GenericMenu menu = new GenericMenu();

            if (onRemoved != null)
            {
                menu.AddItem(new GUIContent("Remove"), false, () => onRemoved());
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Remove"));
            }

            if (entity != null)
            {
                menu.AddItem(new GUIContent("Copy"), false, () => SystemClipboard.CopyEntity(entity));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Copy"));
            }

            // Paste lands in Phase 6 once we have proper parent-aware paste targets.
            menu.AddDisabledItem(new GUIContent("Paste"));

            menu.ShowAsContext();
        }

        private static HelpLinkAttribute GetHelpLinkAttribute(IEntity entity)
        {
            if (entity == null) return null;

            // The HelpLink is normally on the entity class itself (e.g. ObjectInRangeCondition).
            // Fall back to its Data type for entities that wrap their config on Data.
            HelpLinkAttribute fromEntity = entity.GetType().GetCustomAttribute<HelpLinkAttribute>(inherit: true);
            if (fromEntity != null) return fromEntity;

            if (entity is IDataOwner owner && owner.Data != null)
            {
                return owner.Data.GetType().GetCustomAttribute<HelpLinkAttribute>(inherit: true);
            }

            return null;
        }
    }
}
