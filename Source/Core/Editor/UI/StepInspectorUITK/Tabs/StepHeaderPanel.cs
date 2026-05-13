// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs
{
    internal sealed class StepHeaderPanel : IStepInspectorPanel
    {
        public string Id => PanelIds.Header;
        public GUIContent Label { get; } = new GUIContent("Step");

        public VisualElement BuildContent(IStepData step, IElementDrawerContext ctx)
        {
            ScrollView root = new ScrollView(ScrollViewMode.Vertical);
            root.AddToClassList("vrb-step-header");

            root.Add(BuildNameField(step));
            root.Add(BuildDescriptionField(step));

            return root;
        }

        public void Refresh() { }
        public void Dispose() { }

        private static TextField BuildNameField(IStepData step)
        {
            string capturedOld = step.Name ?? string.Empty;

            TextField field = new TextField("Step Name")
            {
                value = capturedOld,
                isDelayed = true,
                multiline = false
            };
            field.AddToClassList("vrb-step-header__name");

            field.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                string newValue = evt.newValue ?? string.Empty;
                string oldValue = capturedOld;
                if (newValue == oldValue)
                {
                    return;
                }

                TabMutations.Do(
                    () => step.SetName(newValue),
                    () => step.SetName(oldValue));

                capturedOld = newValue;
            });

            return field;
        }

        private static TextField BuildDescriptionField(IStepData step)
        {
            string capturedOld = step.Description ?? string.Empty;

            TextField field = new TextField("Description")
            {
                value = capturedOld,
                isDelayed = true,
                multiline = true
            };
            field.AddToClassList("vrb-step-header__description");
            field.style.whiteSpace = WhiteSpace.Normal;

            field.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                string newValue = evt.newValue ?? string.Empty;
                string oldValue = capturedOld;
                if (newValue == oldValue)
                {
                    return;
                }

                TabMutations.Do(
                    () => step.Description = newValue,
                    () => step.Description = oldValue);

                capturedOld = newValue;
            });

            return field;
        }
    }
}
