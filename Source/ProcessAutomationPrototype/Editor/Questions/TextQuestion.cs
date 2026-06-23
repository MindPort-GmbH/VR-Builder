using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.ProcessAutomationPrototype.Editor.Questions
{
    /// <summary>Text input question.</summary>
    public class TextQuestion : IWizardQuestion
    {
        private readonly string defaultValue;

        public string Title { get; }

        public TextQuestion(string title, string defaultValue)
        {
            Title = title;
            this.defaultValue = defaultValue;
        }

        public void BuildComposer(VisualElement composer, object prefill, Action<object, string> submit, Action<string> error)
        {
            TextField field = new TextField { value = prefill as string ?? defaultValue };
            field.AddToClassList("pw-input");
            field.style.flexGrow = 1;

            void Send()
            {
                string value = field.value?.Trim();
                if (string.IsNullOrEmpty(value))
                {
                    error?.Invoke("Please type a name.");
                    return;
                }

                submit(value, value);
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
