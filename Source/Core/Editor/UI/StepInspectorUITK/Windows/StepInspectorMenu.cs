// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    internal static class StepInspectorMenu
    {
        private const string Root = "Tools/VR Builder/Step Inspector (UITK)/";

        // ───────── single-container entry point ─────────

        [MenuItem(Root + "Open All", priority = 200)]
        public static void OpenAll()
        {
            // 1. Tear down any existing UITK panel windows so we get a clean slate.
            //    Otherwise a floating window left over from a previous session + the
            //    re-docked instance would leave the user staring at two copies.
            foreach (DetachedPanelWindow existing in
                UnityEngine.Resources.FindObjectsOfTypeAll<DetachedPanelWindow>())
            {
                if (existing != null)
                {
                    existing.Close();
                }
            }

            // 2. Open the anchor (Step) as a normal floating window so it has a host view.
            //    Set the position now, while it's still a lone DockArea in its ContainerWindow,
            //    so the container ends up sized for the eventual 3-tab strip below.
            DetachedPanelWindow anchor = DetachedPanelWindow.OpenOrFocus(PanelIds.Header);
            ResizeAnchorContainer(anchor);

            // 3. Build the other three without Show() so they never get their own floating
            //    ContainerWindow — they'll be hosted by the new DockArea created in SplitBelow.
            DetachedPanelWindow behaviors = DetachedPanelWindow.CreateForDocking(PanelIds.Behaviors);
            DetachedPanelWindow transitions = DetachedPanelWindow.CreateForDocking(PanelIds.Transitions);
            DetachedPanelWindow unlocked = DetachedPanelWindow.CreateForDocking(PanelIds.Unlocked);

            // 4. Split anchor's container vertically: Step stays on top, the three sibling
            //    panels become tabs of a new DockArea below. If the reflection-based split
            //    fails on this Unity version, fall back to tab-docking all four in one
            //    container — the user can drag-split manually.
            bool split = WindowDockingHelper.SplitBelow(anchor,
                new EditorWindow[] { behaviors, transitions, unlocked });

            if (!split)
            {
                if (!WindowDockingHelper.DockAsTab(anchor, behaviors)) behaviors.Show();
                if (!WindowDockingHelper.DockAsTab(anchor, transitions)) transitions.Show();
                if (!WindowDockingHelper.DockAsTab(anchor, unlocked)) unlocked.Show();
            }

            // Re-apply the desired container size after the layout settles — split-docking
            // can cause Unity to re-fit the container to the minSize of inserted views.
            ResizeAnchorContainer(anchor);

            anchor.Focus();
        }

        private static void ResizeAnchorContainer(EditorWindow anchor)
        {
            if (anchor == null) return;

            Rect target = ComputeDefaultRect();

            // Plain assignment only resizes the panel-within-dock — set the underlying
            // ContainerWindow position via reflection so the actual OS window grows.
            if (TrySetContainerWindowPosition(anchor, target)) return;

            try { anchor.position = target; } catch { /* docked configurations may throw */ }
        }

        private static Rect ComputeDefaultRect()
        {
            try
            {
                Rect main = EditorGUIUtility.GetMainWindowPosition();
                float width = Mathf.Max(720f, main.width * 0.45f);
                float height = Mathf.Max(640f, main.height * 0.75f);
                float x = main.x + (main.width - width) * 0.5f;
                float y = main.y + (main.height - height) * 0.5f;
                return new Rect(x, y, width, height);
            }
            catch
            {
                return new Rect(150f, 150f, 820f, 720f);
            }
        }

        private static bool TrySetContainerWindowPosition(EditorWindow window, Rect rect)
        {
            try
            {
                System.Reflection.FieldInfo parentField = typeof(EditorWindow).GetField(
                    "m_Parent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                object host = parentField?.GetValue(window);
                if (host == null) return false;

                // HostView → ContainerWindow: walk the type hierarchy to find a "window" member.
                System.Reflection.PropertyInfo windowProp = null;
                for (System.Type t = host.GetType(); t != null; t = t.BaseType)
                {
                    windowProp = t.GetProperty("window",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (windowProp != null) break;
                }
                object container = windowProp?.GetValue(host);
                if (container == null) return false;

                System.Reflection.PropertyInfo posProp = container.GetType().GetProperty(
                    "position",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (posProp == null || posProp.CanWrite == false) return false;

                posProp.SetValue(container, rect);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ───────── per-panel entry points (focus existing, otherwise open) ─────────

        [MenuItem(Root + "Step", priority = 220)]
        public static void OpenHeader() => DetachedPanelWindow.OpenOrFocus(PanelIds.Header);

        [MenuItem(Root + "Behaviors", priority = 221)]
        public static void OpenBehaviors() => DetachedPanelWindow.OpenOrFocus(PanelIds.Behaviors);

        [MenuItem(Root + "Transitions", priority = 222)]
        public static void OpenTransitions() => DetachedPanelWindow.OpenOrFocus(PanelIds.Transitions);

        [MenuItem(Root + "Unlocked Objects", priority = 223)]
        public static void OpenUnlocked() => DetachedPanelWindow.OpenOrFocus(PanelIds.Unlocked);

        // To open another instance of an already-open panel, use the "+" button on that
        // panel's header — see PanelHost.cs (calls DetachedPanelWindow.OpenNew).
    }
}
