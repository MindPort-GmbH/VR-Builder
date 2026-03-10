// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

namespace VRBuilder.Core.UI.Drawers.Metadata
{
    /// <summary>
    /// Pending reorder action for an item in a reorderable list.
    /// </summary>
    public enum ReorderAction
    {
        None,
        MoveToTop,
        MoveUp,
        MoveDown
    }

    /// <summary>
    /// Metadata to make <see cref="VRBuilder.Core.Attributes.ReorderableListOfAttribute"/> reorderable.
    /// </summary>
    public class ReorderableElementMetadata
    {
        /// <summary>
        /// Reorder request queued by the UI and consumed by the reorder processing loop.
        /// </summary>
        public ReorderAction PendingAction { get; set; }

        /// <summary>
        /// Determines, whether the entity is the first one in the list.
        /// </summary>
        public bool IsFirst { get; set; }

        /// <summary>
        /// Determines, whether the entity is the last one in the list.
        /// </summary>
        public bool IsLast { get; set; }
    }
}
