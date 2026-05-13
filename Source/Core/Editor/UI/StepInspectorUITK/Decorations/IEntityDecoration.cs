// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Decorations
{
    /// <summary>
    /// Pluggable extension that appends extra UI (toggles, buttons, hints, …) to a behavior,
    /// condition, or transition body when its data matches a predicate. Discovered automatically
    /// by <see cref="EntityDecorationRegistry"/> — no registration needed beyond implementing
    /// this interface on a concrete class.
    /// </summary>
    public interface IEntityDecoration
    {
        /// <summary>Lower numbers render earlier; default 0.</summary>
        int Order { get; }

        /// <summary>Returns true if this decoration applies to <paramref name="entity"/>.</summary>
        bool AppliesTo(object entity);

        /// <summary>
        /// Builds the decoration UI. <paramref name="onChanged"/> should be invoked after a
        /// mutation so the host can react (e.g. graph view refresh) without triggering a full
        /// rebuild of the panel.
        /// </summary>
        VisualElement Build(object entity, Action onChanged);
    }
}
