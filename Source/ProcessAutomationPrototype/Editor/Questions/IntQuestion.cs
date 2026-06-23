using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.ProcessAutomationPrototype.Editor.Questions
{
    /// <summary>Integer input question.</summary>
    public class IntQuestion : IWizardQuestion
    {
        private readonly int min;
        private readonly int defaultValue;

        public string Title { get; }

        public IntQuestion(string title, int min, int defaultValue)
        {
            Title = title;
            this.min = min;
            this.defaultValue = defaultValue;
        }

        public void BuildComposer(VisualElement composer, object prefill, Action<object, string> submit, Action<string> error)
        {
            int initial = prefill is int prefilled ? prefilled : defaultValue;
            IntegerField field = new IntegerField { value = initial };
            field.AddToClassList("pw-input");
            field.style.flexGrow = 1;

            void Send()
            {
                int value = field.value;
                if (value < min)
                {
                    error?.Invoke($"Please enter a number of at least {min}.");
                    return;
                }

                submit(value, value.ToString(CultureInfo.InvariantCulture));
            }

            field.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    Send();
                    evt.StopPropagation();
                }
            });

            Button send = new Button(Send) { text = "Send" };
            send.AddToClassList("pw-btn");
            send.AddToClassList("pw-btn--primary");
            send.AddToClassList("pw-send");

            composer.Add(field);
            composer.Add(send);

            field.schedule.Execute(() => field.Focus()).ExecuteLater(16);
        }
    }
}
