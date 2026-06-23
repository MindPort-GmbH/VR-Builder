using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.ProcessAutomationPrototype.Editor.AI;
using VRBuilder.ProcessAutomationPrototype.Editor.Model;

namespace VRBuilder.ProcessAutomationPrototype.Editor
{
    /// <summary>
    /// Editor window for creating VR Builder processes through guided questions or a text prompt.
    /// </summary>
    public class ProcessWizardWindow : EditorWindow
    {
        private const string StyleSheetPath = "Packages/co.mindport.vrbuilder.core/Source/ProcessAutomationPrototype/Editor/ProcessWizard.uss";

        private MenuCatalog catalog;
        private WizardController controller;
        private LlmSettings llmSettings;

        private ScrollView conversation;
        private VisualElement composer;

        private ProcessBlueprint pendingBlueprint;

        private TextField cfgAnthropicKey;
        private TextField cfgAnthropicModel;
        private TextField cfgOpenAiBase;
        private TextField cfgOpenAiModel;
        private TextField cfgOpenAiKey;

        [MenuItem("Tools/VR Builder/Process Wizard...", false, 16)]
        private static void Open()
        {
            ProcessWizardWindow window = GetWindow<ProcessWizardWindow>();
            window.titleContent = new GUIContent("Process Wizard");
            window.minSize = new Vector2(480f, 480f);
        }

        private void CreateGUI()
        {
            catalog = new MenuCatalog();
            controller = new WizardController(catalog);
            llmSettings = LlmSettings.Load();

            VisualElement root = rootVisualElement;
            root.AddToClassList("pw-root");

            StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (sheet != null)
            {
                root.styleSheets.Add(sheet);
            }

            Label header = new Label("Process Wizard");
            header.AddToClassList("pw-header");
            root.Add(header);

            Label subtitle = new Label("Build a VR Builder process by answering questions or describing it in a prompt.");
            subtitle.AddToClassList("pw-subtitle");
            root.Add(subtitle);

            conversation = new ScrollView(ScrollViewMode.Vertical);
            conversation.AddToClassList("pw-conversation");
            conversation.style.flexGrow = 1;
            conversation.style.flexBasis = 0;
            conversation.style.minHeight = 0;
            root.Add(conversation);

            composer = new VisualElement();
            composer.AddToClassList("pw-composer");
            composer.style.flexShrink = 0;
            root.Add(composer);

            ShowIntro();
        }

        private void ShowIntro()
        {
            AddMessage("Hi! I can build a VR Builder process two ways: I can walk you through it with a few questions, or you can just describe what you want and I'll generate it.", false);

            composer.Clear();

            VisualElement row = ComposerRow();

            Button guided = new Button(StartGuided) { text = "Guided" };
            guided.AddToClassList("pw-btn");
            guided.AddToClassList("pw-btn--primary");
            guided.AddToClassList("pw-grow");
            row.Add(guided);

            Button prompt = new Button(StartPrompt) { text = "From a prompt" };
            prompt.AddToClassList("pw-btn");
            prompt.AddToClassList("pw-grow");
            prompt.AddToClassList("pw-gap");
            row.Add(prompt);

            composer.Add(row);
        }

        private void StartGuided()
        {
            AddMessage("Let's do it step by step.", true);
            controller.Start();
            AskCurrent();
        }

        private void AskCurrent()
        {
            AddMessage(controller.Current.Title, false);
            BuildComposerForCurrent(null);
            ScrollToBottom();
        }

        private void HandleAnswer(object value, string label)
        {
            AddMessage(label, true);
            controller.Submit(value);

            if (controller.IsFinished)
            {
                pendingBlueprint = controller.Blueprint;
                AddMessage(SummaryText(pendingBlueprint), false);
            }
            else
            {
                AddMessage(controller.Current.Title, false);
            }

            BuildComposerForCurrent(null);
            ScrollToBottom();
        }

        private void OnUndo()
        {
            object prefill = controller.Back();
            RemoveLastMessage();
            RemoveLastMessage();
            BuildComposerForCurrent(prefill);
            ScrollToBottom();
        }

        private void BuildComposerForCurrent(object prefill)
        {
            composer.Clear();

            Label error = NewErrorLabel();
            composer.Add(error);

            VisualElement row = ComposerRow();

            if (controller.CanGoBack)
            {
                row.Add(UndoButton());
            }

            if (controller.IsFinished)
            {
                row.Add(GenerateButton());
            }
            else
            {
                Action<string> showError = message => ShowError(error, message);
                controller.Current.BuildComposer(row, prefill, HandleAnswer, showError);
            }

            composer.Add(row);
        }

        private void StartPrompt()
        {
            AddMessage("Describe the process you'd like — for example: \"The user grabs a wrench, uses it on the bolt, then presses the green button to finish.\" I'll generate the chapters, steps, behaviors and transitions.", false);
            BuildPromptComposer();
            ScrollToBottom();
        }

        private void BuildPromptComposer()
        {
            composer.Clear();
            cfgAnthropicKey = cfgAnthropicModel = cfgOpenAiBase = cfgOpenAiModel = cfgOpenAiKey = null;

            Label error = NewErrorLabel();
            composer.Add(error);

            List<string> engineChoices = new List<string> { "Offline (no API key)", "Claude (Anthropic)", "OpenAI-compatible" };
            DropdownField engineDropdown = new DropdownField("Engine", engineChoices, (int)llmSettings.Engine);
            engineDropdown.AddToClassList("pw-engine");
            engineDropdown.RegisterValueChangedCallback(_ =>
            {
                CaptureEngineConfig();
                llmSettings.Engine = (LlmEngine)engineDropdown.index;
                llmSettings.Save();
                BuildPromptComposer();
            });
            composer.Add(engineDropdown);

            if (llmSettings.Engine == LlmEngine.Anthropic)
            {
                cfgAnthropicModel = ConfigField("Model", llmSettings.AnthropicModelOrDefault(), false);
                cfgAnthropicKey = ConfigField("API key", llmSettings.AnthropicKey, true);
                composer.Add(cfgAnthropicModel);
                composer.Add(cfgAnthropicKey);
            }
            else if (llmSettings.Engine == LlmEngine.OpenAi)
            {
                cfgOpenAiBase = ConfigField("Base URL", llmSettings.OpenAiBaseUrlOrDefault(), false);
                cfgOpenAiModel = ConfigField("Model", llmSettings.OpenAiModel, false);
                cfgOpenAiKey = ConfigField("API key", llmSettings.OpenAiKey, true);
                composer.Add(cfgOpenAiBase);
                composer.Add(cfgOpenAiModel);
                composer.Add(cfgOpenAiKey);
            }
            else
            {
                composer.Add(new Label("Offline mode builds a process from keywords in your description — no API key or internet needed. Pick an engine above to use an AI model for richer results.")
                {
                    style = { whiteSpace = WhiteSpace.Normal, marginTop = 4, marginBottom = 4, color = new Color(0.55f, 0.55f, 0.6f) }
                });
            }

            VisualElement row = ComposerRow();

            TextField field = new TextField { multiline = true };
            field.AddToClassList("pw-input");
            field.style.flexGrow = 1;
            field.style.minHeight = 54;
            row.Add(field);

            Button send = new Button(() =>
            {
                CaptureEngineConfig();
                llmSettings.Save();
                OnPromptSend(field.value, error);
            })
            { text = "Generate" };
            send.AddToClassList("pw-btn");
            send.AddToClassList("pw-btn--primary");
            send.AddToClassList("pw-send");
            row.Add(send);

            composer.Add(row);

            field.schedule.Execute(() => field.Focus()).ExecuteLater(16);
        }

        private void CaptureEngineConfig()
        {
            if (cfgAnthropicModel != null)
            {
                llmSettings.AnthropicModel = cfgAnthropicModel.value;
            }

            if (cfgAnthropicKey != null)
            {
                llmSettings.AnthropicKey = cfgAnthropicKey.value;
            }

            if (cfgOpenAiBase != null)
            {
                llmSettings.OpenAiBaseUrl = cfgOpenAiBase.value;
            }

            if (cfgOpenAiModel != null)
            {
                llmSettings.OpenAiModel = cfgOpenAiModel.value;
            }

            if (cfgOpenAiKey != null)
            {
                llmSettings.OpenAiKey = cfgOpenAiKey.value;
            }
        }

        private void OnPromptSend(string text, Label error)
        {
            string request = text?.Trim();
            if (string.IsNullOrEmpty(request))
            {
                ShowError(error, "Please describe the process first.");
                return;
            }

            AddMessage(request, true);
            AddMessage(llmSettings.Engine == LlmEngine.None ? "Building your process…" : "Generating your process… this can take a few seconds.", false);
            ShowBusyComposer();
            ScrollToBottom();

            ProcessPromptService.Generate(llmSettings, catalog, request,
                blueprint =>
                {
                    RemoveLastMessage();
                    pendingBlueprint = blueprint;
                    AddMessage(SummaryText(blueprint), false);
                    ShowPromptGenerateComposer();
                    ScrollToBottom();
                },
                message =>
                {
                    RemoveLastMessage();
                    AddMessage($"⚠ I couldn't generate that: {message}", false);
                    BuildPromptComposer();
                    ScrollToBottom();
                });
        }

        private void ShowBusyComposer()
        {
            composer.Clear();
            Button busy = new Button { text = "Working…" };
            busy.SetEnabled(false);
            busy.AddToClassList("pw-btn");
            busy.AddToClassList("pw-grow");
            VisualElement row = ComposerRow();
            row.Add(busy);
            composer.Add(row);
        }

        private void ShowPromptGenerateComposer()
        {
            composer.Clear();

            VisualElement row = ComposerRow();

            Button again = new Button(BuildPromptComposer) { text = "New prompt" };
            again.AddToClassList("pw-btn");
            again.style.marginRight = 8;
            row.Add(again);

            row.Add(GenerateButton());

            composer.Add(row);
        }

        private void OnGenerate()
        {
            if (pendingBlueprint == null)
            {
                return;
            }

            ProcessBlueprintBuilder.BuildSaveAndOpen(pendingBlueprint, catalog);
            Close();
        }

        private Button GenerateButton()
        {
            Button generate = new Button(OnGenerate) { text = "Generate process" };
            generate.AddToClassList("pw-btn");
            generate.AddToClassList("pw-btn--primary");
            generate.AddToClassList("pw-grow");
            return generate;
        }

        private Button UndoButton()
        {
            Button undo = new Button(OnUndo) { text = "↩", tooltip = "Undo last answer" };
            undo.AddToClassList("pw-btn");
            undo.AddToClassList("pw-undo");
            return undo;
        }

        private static TextField ConfigField(string label, string value, bool password)
        {
            TextField field = new TextField(label) { value = value ?? string.Empty, isPasswordField = password };
            field.AddToClassList("pw-config");
            return field;
        }

        private string SummaryText(ProcessBlueprint blueprint)
        {
            int stepCount = blueprint.Chapters.Sum(chapter => chapter.Steps.Count);
            int behaviorCount = blueprint.Chapters.Sum(chapter => chapter.Steps.Sum(step => step.BehaviorPaths.Count));
            int transitionCount = blueprint.Chapters.Sum(chapter => chapter.Steps.Sum(step => step.Transitions.Count));

            return "Here's what I'll create:\n" +
                   $"•  Process: {blueprint.ProcessName}\n" +
                   $"•  Chapters: {blueprint.Chapters.Count}\n" +
                   $"•  Steps: {stepCount}\n" +
                   $"•  Behaviors: {behaviorCount}\n" +
                   $"•  Transitions: {transitionCount}\n\n" +
                   "Scene-object references are left empty for you to assign later. Hit Generate and I'll open it in the Process Editor.";
        }

        private static VisualElement ComposerRow()
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("pw-composer-row");
            return row;
        }

        private static Label NewErrorLabel()
        {
            Label error = new Label();
            error.AddToClassList("pw-error");
            error.style.display = DisplayStyle.None;
            return error;
        }

        private static void ShowError(Label error, string message)
        {
            error.text = message;
            error.style.display = DisplayStyle.Flex;
        }

        private void AddMessage(string text, bool isUser)
        {
            VisualElement wrapper = new VisualElement();
            wrapper.AddToClassList("pw-msg");
            wrapper.AddToClassList(isUser ? "pw-msg--user" : "pw-msg--assistant");

            Label role = new Label(isUser ? "You" : "VR Builder");
            role.AddToClassList("pw-role");
            wrapper.Add(role);

            Label bubble = new Label(text);
            bubble.AddToClassList("pw-bubble");
            bubble.AddToClassList(isUser ? "pw-bubble--user" : "pw-bubble--assistant");
            wrapper.Add(bubble);

            conversation.contentContainer.Add(wrapper);
        }

        private void RemoveLastMessage()
        {
            VisualElement content = conversation.contentContainer;
            if (content.childCount > 0)
            {
                content.RemoveAt(content.childCount - 1);
            }
        }

        private void ScrollToBottom()
        {
            rootVisualElement.schedule.Execute(() =>
            {
                conversation.scrollOffset = new Vector2(0f, conversation.contentContainer.layout.height);
            }).ExecuteLater(16);
        }
    }
}
