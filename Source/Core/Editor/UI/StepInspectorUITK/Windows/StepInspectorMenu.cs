// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEditor;
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
            //    Otherwise a floating window left over from a previous session + a
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
            DetachedPanelWindow anchor = DetachedPanelWindow.OpenOrFocus(PanelIds.Header);

            // 3. For the other three, CreateForDocking returns a window WITHOUT calling
            //    Show() — so no stray floating ContainerWindow is created. If the
            //    reflection-based AddTab succeeds, the window becomes a tab of the anchor's
            //    container. If it fails, fall back to Show() so the window still appears
            //    (just free-floating).
            CreateOrShow(PanelIds.Behaviors, anchor);
            CreateOrShow(PanelIds.Transitions, anchor);
            CreateOrShow(PanelIds.Unlocked, anchor);

            anchor.Focus();
        }

        private static void CreateOrShow(string panelId, EditorWindow anchor)
        {
            DetachedPanelWindow window = DetachedPanelWindow.CreateForDocking(panelId);
            if (!WindowDockingHelper.DockAsTab(anchor, window))
            {
                window.Show();
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
