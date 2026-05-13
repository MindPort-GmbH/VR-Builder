// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Decorations
{
    /// <summary>
    /// Reflection-discovered registry of <see cref="IEntityDecoration"/> instances.
    /// To add a new decoration, simply create a public class that implements
    /// <see cref="IEntityDecoration"/> — it will be picked up at editor load.
    /// </summary>
    [InitializeOnLoad]
    public static class EntityDecorationRegistry
    {
        private static readonly List<IEntityDecoration> decorations;

        static EntityDecorationRegistry()
        {
            decorations = ReflectionUtils
                .GetConcreteImplementationsOf<IEntityDecoration>()
                .Select(t =>
                {
                    try
                    {
                        return (IEntityDecoration)ReflectionUtils.CreateInstanceOfType(t);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(d => d != null)
                .OrderBy(d => d.Order)
                .ToList();
        }

        /// <summary>
        /// Appends every applicable decoration's UI to <paramref name="container"/>.
        /// Call this after building the auto-drawn body of a behavior/condition/transition.
        /// </summary>
        public static void AppendApplicable(VisualElement container, object entity, Action onChanged)
        {
            if (container == null || entity == null)
            {
                return;
            }

            foreach (IEntityDecoration decoration in decorations)
            {
                if (!decoration.AppliesTo(entity))
                {
                    continue;
                }

                VisualElement ui = decoration.Build(entity, onChanged);
                if (ui != null)
                {
                    container.Add(ui);
                }
            }
        }
    }
}
