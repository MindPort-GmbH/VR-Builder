// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System.Collections.Generic;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items
{
    /// <summary>
    /// In-memory store of per-entity foldout expanded/collapsed state.
    /// </summary>
    /// <remarks>
    /// Structural mutations (reorder, add, remove) cause the panel to rebuild — every
    /// <see cref="CollapsibleItem"/> is recreated. Without this store the new rows would
    /// fall back to <c>startExpanded</c> and visually pop open / closed. The dictionary
    /// only contains entries the user has toggled; unmodified rows keep their default state.
    /// Entries are keyed by the entity instance (or any opaque token) and persist for the
    /// lifetime of the editor.
    /// </remarks>
    internal static class FoldoutState
    {
        private static readonly Dictionary<object, bool> states = new Dictionary<object, bool>();

        public static bool? Get(object key)
        {
            if (key == null) return null;
            return states.TryGetValue(key, out bool stored) ? stored : (bool?)null;
        }

        public static void Set(object key, bool expanded)
        {
            if (key == null) return;
            states[key] = expanded;
        }

        public static void Clear(object key)
        {
            if (key == null) return;
            states.Remove(key);
        }
    }
}
