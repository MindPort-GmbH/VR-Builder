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

        [MenuItem(Root + "Open All", priority = 200)]
        public static void OpenAll()
        {
            foreach (string panelId in PanelIds.AllInOrder)
            {
                DetachedPanelWindow.OpenOrFocus(panelId);
            }
        }

        [MenuItem(Root + "Step", priority = 220)]
        public static void OpenHeader() => DetachedPanelWindow.OpenOrFocus(PanelIds.Header);

        [MenuItem(Root + "Behaviors", priority = 221)]
        public static void OpenBehaviors() => DetachedPanelWindow.OpenOrFocus(PanelIds.Behaviors);

        [MenuItem(Root + "Transitions", priority = 222)]
        public static void OpenTransitions() => DetachedPanelWindow.OpenOrFocus(PanelIds.Transitions);

        [MenuItem(Root + "Unlocked Objects", priority = 223)]
        public static void OpenUnlocked() => DetachedPanelWindow.OpenOrFocus(PanelIds.Unlocked);
    }
}
