using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs
{
    internal sealed class StepHeaderPanel : IStepInspectorPanel
    {
        public string Id => PanelIds.Header;
        public GUIContent Label { get; } = new GUIContent("Step");

        public VisualElement BuildContent(IStepData step)
        {
            ScrollView root = new ScrollView(ScrollViewMode.Vertical);
            root.AddToClassList("vrb-step-header");

            root.Add(BuildNameField(step));
            root.Add(BuildDescriptionField(step));

            return root;
        }

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

            //EnableAutoGrow(field);

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

        // Starts at a single line and grows to fit the wrapped text. Height is recomputed on
        // layout, on commit, and live while typing (InputEvent, since the field is delayed).
        private const float DescriptionLineHeight = 18f;
        private const float DescriptionVerticalPadding = 6f;

        private static void EnableAutoGrow(TextField field)
        {
            void Resize(string text)
            {
                VisualElement input = field.Q(className: "unity-base-field__input");
                TextElement measure = input?.Q<TextElement>();
                if (input == null || measure == null) return;

                float width = measure.resolvedStyle.width;
                if (float.IsNaN(width) || width <= 1f) return;

                string content = string.IsNullOrEmpty(text) ? " " : text;
                float measured = measure.MeasureTextSize(
                    content, width, VisualElement.MeasureMode.Exactly,
                    0f, VisualElement.MeasureMode.Undefined).y;

                input.style.height = Mathf.Max(DescriptionLineHeight, measured) + DescriptionVerticalPadding;
            }

            field.RegisterCallback<GeometryChangedEvent>(_ => Resize(field.value));
            field.RegisterValueChangedCallback(evt => Resize(evt.newValue));
            field.RegisterCallback<InputEvent>(evt => Resize(evt.newData));
        }
    }
}
