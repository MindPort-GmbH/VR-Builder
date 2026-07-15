# Process Wizard (Process Automation Prototype)

Editor window for creating VR Builder processes via a **guided Q&A flow** or an **AI / offline text prompt**. Built on UI Toolkit; lives in `Source/ProcessAutomationPrototype`.

<img width="667" height="589" alt="image" src="https://github.com/user-attachments/assets/7a57228e-8507-4ead-98e6-4325d0a8d8c0" />

## Quick setup

1. This prototype is based on **VR Builder 5.8.0**. If you are using repositories dependent on VR Builder, ensure they are compatible with this version.
2. Check out the **`prototype/automated-process-creation`** branch on **VR Builder** (`VR-Builder`) only — Process Wizard lives there and no extra setup is required beyond opening the Unity project.
3. Open the test project in Unity, wait for compile, then **Tools → VR Builder → Process Wizard...**

Local package paths in the test project `manifest.json` should already point at your clones (e.g. `file:D:/MindPort/VR-Builder`). No defines, samples, or manual steps beyond the branch checkouts above.

## Dependencies

| Requirement | Notes |
|-------------|--------|
| **Unity** | 2022.3+ (project uses Unity 6) |
| **VR Builder Core** | `co.mindport.vrbuilder.core` — assembly `VRBuilder.ProcessAutomationPrototype.Editor` references `VRBuilder.Core` + `VRBuilder.Core.Editor` |
| **`VR_BUILDER` scripting define** | Set when VR Builder is installed (required by asmdef) |
| **`com.unity.nuget.newtonsoft-json`** | Required for AI prompt JSON + session history persistence (`NEWTONSOFT_JSON` define) - set by VR Builder|
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
- Long Promts breake UI 
<img width="672" height="643" alt="image" src="https://github.com/user-attachments/assets/7f6271e6-ef82-4b08-923d-83e7f044ba4f" />

- GPT 5 and newer are Broken `⚠ I couldn't generate that: Request to "gpt-5" failed: 400: Unsupported parameter: 'max_tokens' is not supported with this model. Use 'max_completion_tokens' instead.` 
<img width="415" height="334" alt="image" src="https://github.com/user-attachments/assets/9484bfb9-9eff-41b0-8d18-49df3654db18" />

## Main files

| Path | Purpose |
|------|---------|
| `Editor/ProcessWizardWindow.cs` | Window UI, tabs, history, composers |
| `Editor/WizardController.cs` | Guided question flow |
| `Editor/ProcessBlueprintBuilder.cs` | Blueprint → VR Builder process asset |
| `Editor/AI/ProcessPromptService.cs` | LLM + offline prompt generation |
| `Editor/WizardSessionStore.cs` | History persistence |

## Example
Example Prommt
`Create a process that has 1 chapter with 4 steps.
Step 1 (Name TTS): TTS with "hello, how are you?" and highlight behavior transfers to Step 2.
Step 2 (Name transitions): Two transitions (transition 1 - (grab object condition + grab object condition) -> Step 4, transition 2 - (grab object condition + snap condition + timeout) -> Step 3).
Step 3 (Restart): Transition to Step 1 (transition 1 - (grab object condition)).
Step 4 (Name End 2):`

<img width="926" height="798" alt="image" src="https://github.com/user-attachments/assets/3953c3c0-050f-4431-9c7f-5436ab034979" />
<img width="1384" height="464" alt="image" src="https://github.com/user-attachments/assets/4cc82779-d526-4ece-ba35-a8ff8a51b45c" />


