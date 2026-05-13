// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Instantiators
{
    /// <summary>
    /// Builds a Unity <see cref="GenericMenu"/> from a list of <see cref="MenuOption{T}"/>
    /// (the same data the legacy Add Behavior / Add Condition buttons consume) and shows it.
    /// </summary>
    internal static class AddMenuHelper
    {
        public static void ShowMenu<T>(IList<MenuOption<T>> options, Action<T> onPicked)
        {
            GenericMenu menu = new GenericMenu();

            foreach (MenuOption<T> option in options)
            {
                switch (option)
                {
                    case MenuSeparator<T> separator:
                        menu.AddSeparator(separator.PathToSubmenu ?? string.Empty);
                        break;

                    case DisabledMenuItem<T> disabled:
                        menu.AddDisabledItem(new GUIContent(disabled.Label));
                        break;

                    case MenuItem<T> item:
                        menu.AddItem(new GUIContent(item.DisplayedName), false, () =>
                        {
                            T newInstance = item.GetNewItem();
                            onPicked?.Invoke(newInstance);
                        });
                        break;
                }
            }

            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(new GUIContent("No items available"));
            }

            menu.ShowAsContext();
        }
    }
}
