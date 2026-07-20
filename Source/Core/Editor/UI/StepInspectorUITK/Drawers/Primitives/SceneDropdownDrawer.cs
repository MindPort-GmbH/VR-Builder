using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Primitives
{
    /// <summary>
    /// UIToolkit counterpart of the legacy <c>SceneDropdownDrawer</c>. The class name is
    /// intentionally kept identical so members annotated with
    /// <c>[UsesSpecificProcessDrawer("SceneDropdownDrawer")]</c> resolve to this drawer
    /// through <see cref="ElementDrawerLocator"/>'s partial-name lookup.
    /// </summary>
    internal class SceneDropdownDrawer : ElementDrawer
    {
        private const string NoSceneLabel = "<No scene selected>";

        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            string currentPath = value as string;

            List<string> displayOptions = BuildDisplayOptions(out List<string> paths);
            string currentDisplay = ResolveDisplay(currentPath, paths, displayOptions);

            DropdownField field = new DropdownField(label?.text, displayOptions, currentDisplay)
            {
                tooltip = label?.tooltip
            };
            field.AddToClassList("vrb-field");
            field.AddToClassList("vrb-field--scene-dropdown");

            field.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                if (evt.newValue == evt.previousValue) return;

                int index = displayOptions.IndexOf(evt.newValue);
                string newPath = index <= 0 ? null : paths[index];
                string oldPath = currentPath;

                if (newPath == oldPath) return;

                ChangeValue(
                    getNewValueCallback: () => newPath,
                    getOldValueCallback: () => oldPath,
                    assignValueCallback: changeCallback);

                currentPath = newPath;
            });

            // Rebuild options if the user changes the Build Settings scene list while the
            // panel is alive.
            EditorBuildSettings.sceneListChanged += RebuildOptions;
            field.RegisterCallback<DetachFromPanelEvent>(_ => EditorBuildSettings.sceneListChanged -= RebuildOptions);

            void RebuildOptions()
            {
                List<string> refreshed = BuildDisplayOptions(out List<string> refreshedPaths);
                field.choices = refreshed;
                paths = refreshedPaths;
                field.SetValueWithoutNotify(ResolveDisplay(currentPath, paths, refreshed));
            }

            return field;
        }

        private static List<string> BuildDisplayOptions(out List<string> paths)
        {
            paths = new List<string> { null };
            List<string> displayOptions = new List<string> { NoSceneLabel };

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                paths.Add(scene.path);
                displayOptions.Add(Path.GetFileNameWithoutExtension(scene.path));
            }

            return displayOptions;
        }

        private static string ResolveDisplay(string currentPath, List<string> paths, List<string> displayOptions)
        {
            if (string.IsNullOrEmpty(currentPath))
            {
                return NoSceneLabel;
            }

            int index = paths.IndexOf(currentPath);
            return index >= 0 ? displayOptions[index] : NoSceneLabel;
        }
    }
}
