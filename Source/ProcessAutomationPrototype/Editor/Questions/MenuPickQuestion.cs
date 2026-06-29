using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.ProcessAutomationPrototype.Editor.Questions
{
    /// <summary>A single option in a <see cref="MenuPickQuestion"/>.</summary>
    public class MenuPickOption
    {
        /// <summary>Menu path shown in the picker.</summary>
        public string MenuPath { get; }

        /// <summary>Value recorded as the answer.</summary>
        public object Value { get; }

        /// <summary>Label shown in the conversation after selection.</summary>
        public string Label { get; }

        public MenuPickOption(string menuPath, object value, string label)
        {
            MenuPath = menuPath;
            Value = value;
            Label = label;
        }
    }

    /// <summary>Selection question backed by a native menu.</summary>
    public class MenuPickQuestion : IWizardQuestion
    {
        private readonly List<MenuPickOption> options;
        private readonly string buttonLabel;

        public string Title { get; }

        public object SimulationDefault => options.Count > 0 ? options[0].Value : null;

        public MenuPickQuestion(string title, string buttonLabel, IEnumerable<MenuPickOption> options)
        {
            Title = title;
            this.buttonLabel = buttonLabel;
            this.options = new List<MenuPickOption>(options);
        }

        public void BuildComposer(VisualElement composer, object prefill, Action<object, string> submit, Action<string> error)
        {
            Button picker = new Button { text = $"{buttonLabel}  ▾" };
            picker.AddToClassList("pw-btn");
            picker.AddToClassList("pw-picker");

            picker.clicked += () =>
            {
                if (options.Count == 0)
                {
                    error?.Invoke("No options are available.");
                    return;
                }

                GenericMenu menu = new GenericMenu();
                foreach (MenuPickOption option in options)
                {
                    MenuPickOption captured = option;
                    menu.AddItem(new GUIContent(option.MenuPath), false, () => submit(captured.Value, captured.Label));
                }

                menu.DropDown(picker.worldBound);
            };

            composer.Add(picker);
        }
    }
}
