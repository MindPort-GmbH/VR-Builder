// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Reflection;
using UnityEditor;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    /// <summary>
    /// Reflection wrapper around Unity's internal <c>HostView</c> / <c>DockArea</c>
    /// so we can dock manually-created <see cref="EditorWindow"/> instances together
    /// as tabs of a single container. Once docked the user can drag any tab out,
    /// drop it onto another container, or split-dock it natively.
    /// </summary>
    /// <remarks>
    /// Unity has no public API for docking multiple instances of the same EditorWindow
    /// subclass into the same container. The reflection used here targets names that have
    /// been stable across Unity 2020 through 6.x.
    /// </remarks>
    internal static class WindowDockingHelper
    {
        public static bool DockAsTab(EditorWindow anchor, EditorWindow toDock)
        {
            if (anchor == null || toDock == null || anchor == toDock) return false;

            try
            {
                FieldInfo parentField = typeof(EditorWindow).GetField(
                    "m_Parent", BindingFlags.NonPublic | BindingFlags.Instance);
                if (parentField == null) return false;

                object anchorHost = parentField.GetValue(anchor);
                if (anchorHost == null) return false;

                MethodInfo addTab = FindAddTabMethod(anchorHost.GetType());
                if (addTab == null) return false;

                ParameterInfo[] parameters = addTab.GetParameters();
                object[] args = parameters.Length switch
                {
                    1 => new object[] { toDock },
                    2 => new object[] { toDock, true },
                    _ => null,
                };

                if (args == null) return false;

                addTab.Invoke(anchorHost, args);
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(
                    $"[VR Builder] Programmatic docking failed; falling back to free-floating windows. ({e.GetType().Name}: {e.Message})");
                return false;
            }
        }

        private static MethodInfo FindAddTabMethod(Type hostType)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            for (Type current = hostType; current != null; current = current.BaseType)
            {
                MethodInfo m = current.GetMethod(
                    "AddTab", flags, binder: null,
                    types: new[] { typeof(EditorWindow), typeof(bool) },
                    modifiers: null);
                if (m != null) return m;

                m = current.GetMethod(
                    "AddTab", flags, binder: null,
                    types: new[] { typeof(EditorWindow) },
                    modifiers: null);
                if (m != null) return m;
            }

            return null;
        }
    }
}
