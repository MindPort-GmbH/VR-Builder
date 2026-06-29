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

        private readonly WizardSession guidedSession = new WizardSession(WizardMode.Guided);
        private readonly WizardSession promptSession = new WizardSession(WizardMode.Prompt);

        private MenuCatalog catalog;
        private LlmSettings llmSettings;
        private WizardSessionStore sessionStore;

        private VisualElement toolbar;
        private Button guidedTab;
        private Button promptTab;
        private Button historyToggle;
        private ScrollView conversation;
        private VisualElement composer;
        private VisualElement welcomeComposer;
        private WizardHistoryPanel historyPanel;

        private WizardMode? activeMode;
        private bool historyVisible = true;
        private WizardHistoryFilter historyFilter = WizardHistoryFilter.All;
        private WizardSessionRecord viewingHistoryRecord;

        private TextField promptInputField;
        private TextField cfgAnthropicKey;
        private TextField cfgAnthropicModel;
        private TextField cfgOpenAiBase;
        private TextField cfgOpenAiModel;
        private TextField cfgOpenAiKey;

        private WizardSession ActiveSession => activeMode == WizardMode.Prompt ? promptSession : guidedSession;

        [MenuItem("Tools/VR Builder/Process Wizard...", false, 16)]
        private static void Open()
        {
            ProcessWizardWindow window = GetWindow<ProcessWizardWindow>();
            window.titleContent = new GUIContent("Process Wizard");
            window.minSize = new Vector2(640f, 520f);
        }

        private void OnDisable()
        {
            CapturePromptDraft();
            TryArchiveSession(guidedSession);
            TryArchiveSession(promptSession);
        }

        private void CreateGUI()
        {
            catalog = new MenuCatalog();
            llmSettings = LlmSettings.Load();
            sessionStore = new WizardSessionStore();

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

            toolbar = BuildToolbar();
            root.Add(toolbar);

            VisualElement body = new VisualElement();
            body.AddToClassList("pw-body");
            body.style.flexGrow = 1;
            root.Add(body);

            historyPanel = new WizardHistoryPanel(EnterHistoryView, OnHistoryFilterChanged);
            body.Add(historyPanel.Root);

            VisualElement main = new VisualElement();
            main.AddToClassList("pw-main");
            main.style.flexGrow = 1;
            body.Add(main);

            conversation = new ScrollView(ScrollViewMode.Vertical);
            conversation.AddToClassList("pw-conversation");
            conversation.style.flexGrow = 1;
            conversation.style.flexBasis = 0;
            conversation.style.minHeight = 0;
            main.Add(conversation);

            composer = new VisualElement();
            composer.AddToClassList("pw-composer");
            composer.style.flexShrink = 0;
            main.Add(composer);

            welcomeComposer = new VisualElement();
            welcomeComposer.AddToClassList("pw-composer");
            welcomeComposer.style.flexShrink = 0;
            main.Add(welcomeComposer);

            RefreshHistoryList();
            ShowWelcome();
            RegisterKeyboardShortcuts(root);
        }

        private void RegisterKeyboardShortcuts(VisualElement root)
        {
            root.focusable = true;
            root.RegisterCallback<KeyDownEvent>(OnRootKeyDown);
            root.schedule.Execute(() => root.Focus()).ExecuteLater(32);
        }

        private void OnRootKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Escape)
            {
                return;
            }

            if (viewingHistoryRecord != null)
            {
                ExitHistoryView();
                evt.StopImmediatePropagation();
                return;
            }

            if (historyVisible)
            {
                ToggleHistoryPanel();
                evt.StopImmediatePropagation();
            }
        }

        private VisualElement BuildToolbar()
        {
            VisualElement bar = new VisualElement();
            bar.AddToClassList("pw-toolbar");

            VisualElement left = new VisualElement();
            left.AddToClassList("pw-toolbar-tabs");

            historyToggle = new Button(ToggleHistoryPanel) { text = "☰" };
            historyToggle.AddToClassList("pw-btn");
            historyToggle.AddToClassList("pw-toolbar-history");
            historyToggle.AddToClassList("pw-toolbar-history--active");
            historyToggle.tooltip = "Show or hide session history";
            left.Add(historyToggle);

            guidedTab = new Button(() => RequestMode(WizardMode.Guided)) { text = "Guided" };
            guidedTab.AddToClassList("pw-tab");
            guidedTab.tooltip = "Step-by-step questions";
            left.Add(guidedTab);

            promptTab = new Button(() => RequestMode(WizardMode.Prompt)) { text = "AI Prompt" };
            promptTab.AddToClassList("pw-tab");
            promptTab.tooltip = "Describe the process in natural language";
            left.Add(promptTab);

            bar.Add(left);

            Button newSession = new Button(OnNewSessionClicked) { text = "+ New" };
            newSession.AddToClassList("pw-btn");
            newSession.AddToClassList("pw-toolbar-new");
            newSession.tooltip = "Start a fresh session in the current mode";
            bar.Add(newSession);

            return bar;
        }

        private void ToggleHistoryPanel()
        {
            historyVisible = !historyVisible;
            historyPanel.SetCollapsed(!historyVisible);
            historyToggle.EnableInClassList("pw-toolbar-history--active", historyVisible);
        }

        private void RefreshHistoryList()
        {
            string selectedId = viewingHistoryRecord?.Id;
            historyPanel.Refresh(sessionStore.LoadAll(), historyFilter, selectedId);
        }

        private void OnHistoryFilterChanged(WizardHistoryFilter filter)
        {
            historyFilter = filter;
            RefreshHistoryList();
        }

        private void TryArchiveSession(WizardSession session)
        {
            WizardSessionArchive.TryArchive(session, sessionStore);
            RefreshHistoryList();
        }

        private void UpdateToolbarState()
        {
            guidedTab.EnableInClassList("pw-tab--active", activeMode == WizardMode.Guided && viewingHistoryRecord == null);
            promptTab.EnableInClassList("pw-tab--active", activeMode == WizardMode.Prompt && viewingHistoryRecord == null);
        }

        private void ShowWelcome()
        {
            ExitHistoryView(restoreActive: false);
            activeMode = null;
            UpdateToolbarState();
            ClearConversation();
            conversation.EnableInClassList("pw-conversation--readonly", false);
            composer.style.display = DisplayStyle.None;
            welcomeComposer.style.display = DisplayStyle.Flex;
            welcomeComposer.Clear();
            AddMessage("Hi! I can build a VR Builder process two ways: I can walk you through it with a few questions, or you can just describe what you want and I'll generate it.", false, null);

            Label hint = new Label("Pick a mode below, or use the Guided / AI Prompt tabs anytime. Press Esc to close history or leave a saved session view.");
            hint.AddToClassList("pw-welcome-hint");
            welcomeComposer.Add(hint);

            VisualElement row = ComposerRow();
            row.AddToClassList("pw-welcome-actions");

            VisualElement guidedCard = new VisualElement();
            guidedCard.AddToClassList("pw-welcome-card");
            Label guidedTitle = new Label("Guided");
            guidedTitle.AddToClassList("pw-welcome-card-title");
            guidedCard.Add(guidedTitle);
            Label guidedDesc = new Label("Answer step-by-step questions to define chapters, steps, behaviors, and transitions.");
            guidedDesc.AddToClassList("pw-welcome-card-desc");
            guidedCard.Add(guidedDesc);
            Button guided = new Button(() => RequestMode(WizardMode.Guided)) { text = "Start guided" };
            guided.AddToClassList("pw-btn");
            guided.AddToClassList("pw-btn--primary");
            guided.AddToClassList("pw-grow");
            guidedCard.Add(guided);
            row.Add(guidedCard);

            VisualElement promptCard = new VisualElement();
            promptCard.AddToClassList("pw-welcome-card");
            promptCard.AddToClassList("pw-gap");
            Label promptTitle = new Label("AI Prompt");
            promptTitle.AddToClassList("pw-welcome-card-title");
            promptCard.Add(promptTitle);
            Label promptDesc = new Label("Describe your training flow in plain language and let the wizard draft the process for you.");
            promptDesc.AddToClassList("pw-welcome-card-desc");
            promptCard.Add(promptDesc);
            Button prompt = new Button(() => RequestMode(WizardMode.Prompt)) { text = "Write a prompt" };
            prompt.AddToClassList("pw-btn");
            prompt.AddToClassList("pw-grow");
            promptCard.Add(prompt);
            row.Add(promptCard);

            welcomeComposer.Add(row);
            ScrollToBottom();
            rootVisualElement.schedule.Execute(() => guided.Focus()).ExecuteLater(32);
        }

        private void EnterHistoryView(WizardSessionRecord record)
        {
            if (record == null)
            {
                return;
            }

            CapturePromptDraft();
            viewingHistoryRecord = record;
            activeMode = record.Mode;
            UpdateToolbarState();

            welcomeComposer.style.display = DisplayStyle.None;
            composer.style.display = DisplayStyle.Flex;
            ClearConversation();
            conversation.EnableInClassList("pw-conversation--readonly", true);

            foreach (WizardChatMessageRecord message in record.Messages)
            {
                RenderMessage(message.Text, message.IsUser);
            }

            ShowHistoryComposer(record);
            RefreshHistoryList();
            ScrollToBottom();
        }

        private void ExitHistoryView(bool restoreActive = true)
        {
            if (viewingHistoryRecord == null)
            {
                return;
            }

            viewingHistoryRecord = null;
            conversation.EnableInClassList("pw-conversation--readonly", false);
            RefreshHistoryList();

            if (!restoreActive)
            {
                return;
            }

            if (activeMode.HasValue)
            {
                RestoreSession(ActiveSession);
            }
            else
            {
                ShowWelcome();
            }
        }

        private void ShowHistoryComposer(WizardSessionRecord record)
        {
            composer.Clear();

            Label banner = new Label("Viewing a saved session (read-only). Esc also returns to your live session.");
            banner.AddToClassList("pw-history-banner");
            composer.Add(banner);

            VisualElement row = ComposerRow();

            Button back = new Button(() => ExitHistoryView()) { text = "Back to live session" };
            back.AddToClassList("pw-btn");
            back.tooltip = "Leave this saved session and return to your in-progress Guided or AI Prompt tab (Esc)";
            row.Add(back);

            if (record.Blueprint != null)
            {
                Button generate = new Button(() => OnHistoryGenerate(record)) { text = "Generate process" };
                generate.AddToClassList("pw-btn");
                generate.AddToClassList("pw-btn--primary");
                generate.AddToClassList("pw-grow");
                generate.style.marginLeft = 6;
                generate.tooltip = "Create and open this blueprint in the Process Editor";
                row.Add(generate);
            }

            Button delete = new Button(() => OnHistoryDelete(record)) { text = "Delete" };
            delete.AddToClassList("pw-btn");
            delete.AddToClassList("pw-restart");
            delete.style.marginLeft = 6;
            row.Add(delete);

            composer.Add(row);
        }

        private void OnHistoryGenerate(WizardSessionRecord record)
        {
            if (record?.Blueprint == null)
            {
                return;
            }

            ProcessBlueprintBuilder.BuildSaveAndOpen(record.Blueprint, catalog);
            RenderMessage($"✓ Opened \"{record.Blueprint.ProcessName}\" in the Process Editor.", false);
            ScrollToBottom();
        }

        private void OnHistoryDelete(WizardSessionRecord record)
        {
            if (record == null)
            {
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "Delete saved session?",
                $"Remove \"{WizardSessionArchive.BuildTitle(record)}\" from history? This cannot be undone.",
                "Delete",
                "Cancel");

            if (!confirmed)
            {
                return;
            }

            sessionStore.Delete(record.Id);
            if (viewingHistoryRecord?.Id == record.Id)
            {
                viewingHistoryRecord = null;
                conversation.EnableInClassList("pw-conversation--readonly", false);
                if (activeMode.HasValue)
                {
                    RestoreSession(ActiveSession);
                }
                else
                {
                    ShowWelcome();
                }
            }

            RefreshHistoryList();
        }

        private void RequestMode(WizardMode mode)
        {
            if (activeMode == mode && viewingHistoryRecord == null)
            {
                return;
            }

            if (viewingHistoryRecord != null)
            {
                ExitHistoryView(restoreActive: false);
            }

            if (activeMode.HasValue && ActiveSession.HasProgress)
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Switch mode?",
                    "Your current session will be kept. You can return to it anytime using the toolbar tabs.",
                    "Switch",
                    "Cancel");

                if (!confirmed)
                {
                    return;
                }

                TryArchiveSession(ActiveSession);
            }

            CapturePromptDraft();
            activeMode = mode;
            UpdateToolbarState();
            welcomeComposer.style.display = DisplayStyle.None;
            composer.style.display = DisplayStyle.Flex;
            conversation.EnableInClassList("pw-conversation--readonly", false);
            RestoreSession(ActiveSession);
            FocusActiveComposer();
        }

        private void OnNewSessionClicked()
        {
            if (!activeMode.HasValue || viewingHistoryRecord != null)
            {
                return;
            }

            WizardSession session = ActiveSession;
            if (session.HasProgress)
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Start new session?",
                    "The current session will be saved to history, then cleared.",
                    "Start new",
                    "Cancel");

                if (!confirmed)
                {
                    return;
                }
            }

            CapturePromptDraft();
            TryArchiveSession(session);
            BeginFreshSession(session);
        }

        private void BeginFreshSession(WizardSession session)
        {
            session.Reset();
            ClearConversation();
            welcomeComposer.style.display = DisplayStyle.None;
            composer.style.display = DisplayStyle.Flex;
            conversation.EnableInClassList("pw-conversation--readonly", false);

            if (session.Mode == WizardMode.Guided)
            {
                StartGuided(session, true);
            }
            else
            {
                StartPrompt(session, true);
            }
        }

        private void RestoreSession(WizardSession session)
        {
            ClearConversation();
            welcomeComposer.style.display = DisplayStyle.None;
            composer.style.display = DisplayStyle.Flex;
            conversation.EnableInClassList("pw-conversation--readonly", false);

            foreach (WizardChatMessage message in session.Messages)
            {
                RenderMessage(message.Text, message.IsUser);
            }

            if (session.Mode == WizardMode.Guided)
            {
                RestoreGuidedComposer(session);
            }
            else
            {
                RestorePromptComposer(session);
            }

            ScrollToBottom();
            FocusActiveComposer();
        }

        private void FocusActiveComposer()
        {
            if (viewingHistoryRecord != null || !activeMode.HasValue)
            {
                return;
            }

            rootVisualElement.schedule.Execute(() =>
            {
                if (activeMode == WizardMode.Prompt && promptInputField != null)
                {
                    promptInputField.Focus();
                    return;
                }

                if (activeMode == WizardMode.Guided && composer != null)
                {
                    TextField textField = composer.Q<TextField>();
                    if (textField != null)
                    {
                        textField.Focus();
                        return;
                    }

                    IntegerField intField = composer.Q<IntegerField>();
                    if (intField != null)
                    {
                        intField.Focus();
                        return;
                    }

                    Button button = composer.Q<Button>(className: "pw-picker");
                    button?.Focus();
                }
            }).ExecuteLater(32);
        }

        private void RestoreGuidedComposer(WizardSession session)
        {
            if (session.AwaitingAnother)
            {
                ShowPostGenerateComposer(session);
                return;
            }

            if (!session.GuidedStarted)
            {
                StartGuided(session, true);
                return;
            }

            if (session.PendingBlueprint != null)
            {
                BuildGuidedFinishedComposer(session);
                return;
            }

            if (session.Controller == null)
            {
                ShowGuidedReadOnlyComposer(session);
                return;
            }

            if (session.Controller.IsFinished)
            {
                session.PendingBlueprint = session.Controller.Blueprint;
                BuildGuidedFinishedComposer(session);
            }
            else
            {
                BuildComposerForCurrent(session, null);
            }
        }

        private void ShowGuidedReadOnlyComposer(WizardSession session)
        {
            composer.Clear();
            Label banner = new Label("This guided session was restored from history. Start over to continue answering questions.");
            banner.AddToClassList("pw-history-banner");
            composer.Add(banner);

            VisualElement row = ComposerRow();
            Button restart = new Button(() => RestartGuided(session)) { text = "Start over" };
            restart.AddToClassList("pw-btn");
            restart.AddToClassList("pw-btn--primary");
            restart.AddToClassList("pw-grow");
            row.Add(restart);
            composer.Add(row);
        }

        private void RestorePromptComposer(WizardSession session)
        {
            if (session.AwaitingAnother)
            {
                ShowPostGenerateComposer(session);
                return;
            }

            if (session.PromptIsBusy)
            {
                ShowBusyComposer();
                return;
            }

            if (session.PendingBlueprint != null && session.PromptAwaitingGenerate)
            {
                ShowPromptGenerateComposer(session);
                return;
            }

            BuildPromptComposer(session);
        }

        private void StartGuided(WizardSession session, bool addKickoffMessage)
        {
            session.Controller = new WizardController(catalog);
            session.GuidedStarted = true;
            session.PendingBlueprint = null;
            session.AwaitingAnother = false;

            if (addKickoffMessage)
            {
                AddMessage("Let's do it step by step.", true, session);
            }

            session.Controller.Start();
            AddMessage(session.Controller.Current.Title, false, session);
            BuildComposerForCurrent(session, null);
            ScrollToBottom();
        }

        private void BuildComposerForCurrent(WizardSession session, object prefill)
        {
            composer.Clear();

            Label progress = new Label(session.Controller.ProgressText);
            progress.AddToClassList("pw-progress");
            composer.Add(progress);

            Label error = NewErrorLabel();
            composer.Add(error);

            VisualElement actions = new VisualElement();
            actions.AddToClassList("pw-composer-actions");
            composer.Add(actions);

            if (session.Controller.CanGoBack)
            {
                actions.Add(UndoButton(session));
            }

            Button restart = new Button(() => RestartGuided(session)) { text = "Start over" };
            restart.AddToClassList("pw-btn");
            restart.AddToClassList("pw-restart");
            restart.tooltip = "Clear this guided session and begin again";
            actions.Add(restart);

            VisualElement row = ComposerRow();

            if (session.Controller.IsFinished)
            {
                session.PendingBlueprint = session.Controller.Blueprint;
                row.Add(GenerateButton(session));
            }
            else
            {
                Action<string> showError = message => ShowError(error, message);
                session.Controller.Current.BuildComposer(row, prefill, (value, label) => HandleAnswer(session, value, label), showError);
            }

            composer.Add(row);
            FocusActiveComposer();
        }

        private void BuildGuidedFinishedComposer(WizardSession session)
        {
            composer.Clear();

            if (session.Controller != null)
            {
                Label progress = new Label(session.Controller.ProgressText);
                progress.AddToClassList("pw-progress");
                composer.Add(progress);
            }

            VisualElement actions = new VisualElement();
            actions.AddToClassList("pw-composer-actions");
            composer.Add(actions);

            Button restart = new Button(() => RestartGuided(session)) { text = "Start over" };
            restart.AddToClassList("pw-btn");
            restart.AddToClassList("pw-restart");
            actions.Add(restart);

            VisualElement row = ComposerRow();
            row.Add(GenerateButton(session));
            composer.Add(row);
        }

        private void RestartGuided(WizardSession session)
        {
            if (session.HasProgress)
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Start over?",
                    "The current session will be saved to history, then cleared.",
                    "Start over",
                    "Cancel");

                if (!confirmed)
                {
                    return;
                }
            }

            TryArchiveSession(session);
            session.Reset();
            ClearConversation();
            StartGuided(session, true);
        }

        private void HandleAnswer(WizardSession session, object value, string label)
        {
            AddMessage(label, true, session);
            session.Controller.Submit(value);

            if (session.Controller.IsFinished)
            {
                session.PendingBlueprint = session.Controller.Blueprint;
                AddMessage(SummaryText(session.PendingBlueprint), false, session);
                BuildGuidedFinishedComposer(session);
            }
            else
            {
                AddMessage(session.Controller.Current.Title, false, session);
                BuildComposerForCurrent(session, null);
            }

            ScrollToBottom();
        }

        private void OnUndo(WizardSession session)
        {
            object prefill = session.Controller.Back();
            RemoveLastMessage(session);
            RemoveLastMessage(session);
            session.PendingBlueprint = null;
            BuildComposerForCurrent(session, prefill);
            ScrollToBottom();
        }

        private void StartPrompt(WizardSession session, bool addIntro)
        {
            session.PromptAwaitingGenerate = false;
            session.PendingBlueprint = null;
            session.AwaitingAnother = false;

            if (addIntro)
            {
                AddMessage(
                    "Describe the process you'd like — for example: \"The user grabs a wrench, uses it on the bolt, then presses the green button to finish.\" I'll generate the chapters, steps, behaviors and transitions.",
                    false,
                    session);
            }

            BuildPromptComposer(session);
            ScrollToBottom();
        }

        private void BuildPromptComposer(WizardSession session)
        {
            CapturePromptDraft();
            composer.Clear();
            promptInputField = null;
            cfgAnthropicKey = cfgAnthropicModel = cfgOpenAiBase = cfgOpenAiModel = cfgOpenAiKey = null;
            session.PromptAwaitingGenerate = false;

            Label error = NewErrorLabel();
            composer.Add(error);

            List<string> engineChoices = new List<string> { "Offline (no API key)", "Claude (Anthropic)", "OpenAI-compatible" };
            DropdownField engineDropdown = new DropdownField("Engine", engineChoices, (int)llmSettings.Engine);
            engineDropdown.AddToClassList("pw-engine");
            engineDropdown.RegisterValueChangedCallback(_ =>
            {
                CapturePromptDraft();
                CaptureEngineConfig();
                llmSettings.Engine = (LlmEngine)engineDropdown.index;
                llmSettings.Save();
                BuildPromptComposer(session);
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
                VisualElement offlinePanel = new VisualElement();
                offlinePanel.AddToClassList("pw-offline-panel");

                Label offlineTitle = new Label("Offline mode — no API key needed");
                offlineTitle.AddToClassList("pw-offline-title");
                offlinePanel.Add(offlineTitle);

                Label offlineBody = new Label("Describe your training steps using verbs like grab, use, press, or move. The wizard matches keywords to behaviors and builds a starter process you can refine in the Process Editor.");
                offlineBody.AddToClassList("pw-offline-body");
                offlinePanel.Add(offlineBody);

                composer.Add(offlinePanel);
            }

            VisualElement actions = new VisualElement();
            actions.AddToClassList("pw-composer-actions");
            composer.Add(actions);

            if (session.Messages.Count > 0)
            {
                Button restart = new Button(() => RestartPrompt(session)) { text = "Start over" };
                restart.AddToClassList("pw-btn");
                restart.AddToClassList("pw-restart");
                restart.tooltip = "Clear this prompt session and begin again";
                actions.Add(restart);
            }

            VisualElement row = ComposerRow();

            promptInputField = new TextField { multiline = true, value = session.DraftPromptText ?? string.Empty };
            promptInputField.AddToClassList("pw-input");
            promptInputField.style.flexGrow = 1;
            promptInputField.style.minHeight = 54;

            if (llmSettings.Engine == LlmEngine.None)
            {
                promptInputField.textEdition.placeholder = "e.g. The user grabs a wrench, uses it on the bolt, then presses the green button.";
            }
            else
            {
                promptInputField.textEdition.placeholder = "Describe the VR training flow you want to create…";
            }

            promptInputField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return && (evt.ctrlKey || evt.commandKey))
                {
                    CaptureEngineConfig();
                    llmSettings.Save();
                    OnPromptSend(session, promptInputField.value, error);
                    evt.StopImmediatePropagation();
                }
            });

            row.Add(promptInputField);

            Button send = new Button(() =>
            {
                CaptureEngineConfig();
                llmSettings.Save();
                OnPromptSend(session, promptInputField.value, error);
            })
            { text = "Generate" };
            send.AddToClassList("pw-btn");
            send.AddToClassList("pw-btn--primary");
            send.AddToClassList("pw-send");
            send.tooltip = "Ctrl+Enter to generate";
            row.Add(send);

            composer.Add(row);

            Label shortcutHint = new Label(llmSettings.Engine == LlmEngine.None
                ? "Tip: Ctrl+Enter to generate. Offline mode uses keywords from your description."
                : "Tip: Ctrl+Enter to generate.");
            shortcutHint.AddToClassList("pw-hint");
            composer.Add(shortcutHint);

            promptInputField.schedule.Execute(() => promptInputField.Focus()).ExecuteLater(16);
        }

        private void RestartPrompt(WizardSession session)
        {
            if (session.HasProgress)
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Start over?",
                    "The current session will be saved to history, then cleared.",
                    "Start over",
                    "Cancel");

                if (!confirmed)
                {
                    return;
                }
            }

            TryArchiveSession(session);
            session.Reset();
            ClearConversation();
            StartPrompt(session, true);
        }

        private void CapturePromptDraft()
        {
            if (promptInputField != null && activeMode == WizardMode.Prompt)
            {
                promptSession.DraftPromptText = promptInputField.value ?? string.Empty;
            }
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

        private void OnPromptSend(WizardSession session, string text, Label error)
        {
            string request = text?.Trim();
            if (string.IsNullOrEmpty(request))
            {
                ShowError(error, "Please describe the process first.");
                return;
            }

            session.DraftPromptText = string.Empty;
            AddMessage(request, true, session);
            AddMessage(llmSettings.Engine == LlmEngine.None ? "Building your process…" : "Generating your process… this can take a few seconds.", false, session);
            session.PromptIsBusy = true;
            session.LastFailedPromptText = request;
            ShowBusyComposer();
            ScrollToBottom();

            ProcessPromptService.Generate(llmSettings, catalog, request,
                blueprint =>
                {
                    session.PromptIsBusy = false;
                    session.LastFailedPromptText = string.Empty;
                    RemoveLastMessage(session);
                    session.PendingBlueprint = blueprint;
                    session.PromptAwaitingGenerate = true;
                    AddMessage(SummaryText(blueprint), false, session);
                    ShowPromptGenerateComposer(session);
                    ScrollToBottom();
                },
                message =>
                {
                    session.PromptIsBusy = false;
                    RemoveLastMessage(session);
                    AddMessage($"⚠ I couldn't generate that: {message}", false, session);
                    session.DraftPromptText = session.LastFailedPromptText ?? request;
                    BuildPromptComposer(session);
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

        private void ShowPromptGenerateComposer(WizardSession session)
        {
            composer.Clear();

            VisualElement actions = new VisualElement();
            actions.AddToClassList("pw-composer-actions");
            composer.Add(actions);

            Button restart = new Button(() => RestartPrompt(session)) { text = "Start over" };
            restart.AddToClassList("pw-btn");
            restart.AddToClassList("pw-restart");
            actions.Add(restart);

            VisualElement row = ComposerRow();

            Button again = new Button(() =>
            {
                session.PromptAwaitingGenerate = false;
                session.PendingBlueprint = null;
                BuildPromptComposer(session);
            })
            { text = "New prompt" };
            again.AddToClassList("pw-btn");
            again.style.marginRight = 8;
            row.Add(again);

            row.Add(GenerateButton(session));
            composer.Add(row);
        }

        private void OnGenerate(WizardSession session)
        {
            if (session.PendingBlueprint == null)
            {
                return;
            }

            ProcessBlueprint blueprint = session.PendingBlueprint;
            ProcessBlueprintBuilder.BuildSaveAndOpen(blueprint, catalog);
            session.PendingBlueprint = null;
            session.PromptAwaitingGenerate = false;
            session.AwaitingAnother = true;
            AddMessage($"✓ Opened \"{blueprint.ProcessName}\" in the Process Editor. You can start another process here or switch modes using the toolbar.", false, session);
            TryArchiveSession(session);
            ShowPostGenerateComposer(session);
            ScrollToBottom();
        }

        private void ShowPostGenerateComposer(WizardSession session)
        {
            composer.Clear();
            VisualElement row = ComposerRow();
            Button another = new Button(() => BeginFreshSession(session)) { text = "Start another" };
            another.AddToClassList("pw-btn");
            another.AddToClassList("pw-btn--primary");
            another.AddToClassList("pw-grow");
            row.Add(another);
            composer.Add(row);
        }

        private Button GenerateButton(WizardSession session)
        {
            Button generate = new Button(() => OnGenerate(session)) { text = "Generate process" };
            generate.AddToClassList("pw-btn");
            generate.AddToClassList("pw-btn--primary");
            generate.AddToClassList("pw-grow");
            return generate;
        }

        private Button UndoButton(WizardSession session)
        {
            Button undo = new Button(() => OnUndo(session)) { text = "↩", tooltip = "Undo last answer" };
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

        private void ClearConversation()
        {
            conversation.contentContainer.Clear();
        }

        private void AddMessage(string text, bool isUser, WizardSession session)
        {
            if (session != null)
            {
                session.Messages.Add(new WizardChatMessage(isUser, text));
            }

            RenderMessage(text, isUser);
        }

        private void RenderMessage(string text, bool isUser)
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

        private void RemoveLastMessage(WizardSession session)
        {
            if (session != null && session.Messages.Count > 0)
            {
                session.Messages.RemoveAt(session.Messages.Count - 1);
            }

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
