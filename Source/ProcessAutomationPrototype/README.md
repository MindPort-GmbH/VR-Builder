# Process Wizard (Process Automation Prototype)

Editor window for creating VR Builder processes via a **guided Q&A flow** or an **AI / offline text prompt**. Built on UI Toolkit; lives in `Source/ProcessAutomationPrototype`.

## Quick setup

1. Check out **`develop`** on all VR Builder–dependent repos (e.g. **VR Builder Pro**, test projects like **VRBuilderProT**).
2. Check out the **`prototype`** branch on **VR Builder** (`VR-Builder`) only — Process Wizard lives there and no extra setup is required beyond opening the Unity project.
3. Open the test project in Unity, wait for compile, then **Tools → VR Builder → Process Wizard...**

Local package paths in the test project `manifest.json` should already point at your clones (e.g. `file:D:/MindPort/VR-Builder`). No defines, samples, or manual steps beyond the branch checkouts above.

## Dependencies

| Requirement | Notes |
|-------------|--------|
| **Unity** | 2022.3+ (project uses Unity 6) |
| **VR Builder Core** | `co.mindport.vrbuilder.core` — assembly `VRBuilder.ProcessAutomationPrototype.Editor` references `VRBuilder.Core` + `VRBuilder.Core.Editor` |
| **`VR_BUILDER` scripting define** | Set when VR Builder is installed (required by asmdef) |
| **`com.unity.nuget.newtonsoft-json`** | Required for AI prompt JSON + session history persistence (`NEWTONSOFT_JSON` define) |
| **Internet** | Only for Claude / OpenAI prompt modes (not for Guided or Offline prompt) |

Optional for **AI Prompt** mode:

- **Claude (Anthropic):** API key in the wizard UI, or env var `ANTHROPIC_API_KEY`
- **OpenAI-compatible:** API key in UI, or env var `OPENAI_API_KEY`; optional custom base URL / model

LLM settings are stored in **EditorPrefs** under `VRBuilder.ProcessAutomationPrototype.*`.

## Open the wizard

**Tools → VR Builder → Process Wizard...**

## Quick test checklist

### 1. Compile & open

- [ ] Unity compiles with no errors in `VRBuilder.ProcessAutomationPrototype.Editor`
- [ ] Window opens; welcome screen shows **Guided** and **AI Prompt** cards
- [ ] History sidebar (☰) is visible; empty state message shows when no sessions exist

### 2. Guided mode

- [ ] **Guided** tab → answer a few questions (progress label shows e.g. `Question 3 of ~24`)
- [ ] **↩ Undo** and **Start over** work
- [ ] Switch to **AI Prompt** tab and back — guided session is restored
- [ ] Finish wizard → summary → **Generate process** → Process Editor opens with a new process
- [ ] Wizard stays open; **Start another** works

### 3. AI Prompt — offline (no API key)

- [ ] **AI Prompt** tab → engine **Offline (no API key)**
- [ ] Enter e.g. `The user grabs a wrench, uses it on the bolt, then presses the green button.`
- [ ] **Generate** (or **Ctrl+Enter**) → summary → **Generate process** → process opens
- [ ] On failure, prompt text is restored in the input field

### 4. AI Prompt — online (optional)

- [ ] Select **Claude** or **OpenAI-compatible**, enter API key (or use env var)
- [ ] Generate from a short process description → summary → **Generate process**

### 5. History

- [ ] After **Start over**, **+ New**, **Generate process**, or closing the window, a session appears in **History**
- [ ] Filter **All / Guided / Prompt**
- [ ] Click a session → read-only transcript
- [ ] **Generate process** opens the saved blueprint in the Process Editor
- [ ] **Back to live session** (or **Esc**) returns to the active tab
- [ ] **Delete** removes the entry

History file: `Library/VRBuilder/ProcessWizardSessions/sessions.json` (per Unity project, max 50 sessions).

### 6. Keyboard

- [ ] **Esc** — exit history view, or hide history sidebar
- [ ] **Ctrl+Enter** — generate from AI prompt field
- [ ] **Enter** — send guided text / integer answers

## Known limitations

- Transition targets must be **within the same chapter**; invalid or cross-chapter targets are cleared before build (see Console for warnings).
- Scene-object references on behaviors/conditions are **not** set — assign them in the Process Editor after generate.
- History is **read-only** (review + generate again); it does not resume an in-progress guided controller state.
- Guided + AI sessions are kept separately in memory per tab until the window is closed.

## Main files

| Path | Purpose |
|------|---------|
| `Editor/ProcessWizardWindow.cs` | Window UI, tabs, history, composers |
| `Editor/WizardController.cs` | Guided question flow |
| `Editor/ProcessBlueprintBuilder.cs` | Blueprint → VR Builder process asset |
| `Editor/AI/ProcessPromptService.cs` | LLM + offline prompt generation |
| `Editor/WizardSessionStore.cs` | History persistence |
