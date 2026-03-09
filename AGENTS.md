# AGENTS.md
This file provides guidance to AI agents when working with code in this repository.

## Project Overview
This repository contains VR Builder Core, a Unity package for authoring VR applications via graphical workflow editor.

**Package**: `co.mindport.vrbuilder.core`

**Key Architecture Components:**
- VR Builder is extended by VR Builder Pro in a seperate repository Package `co.mindport.vrbuilder.tests` containing its own `AGENT.md`.
- Tests are not in this repository but in a seperate repository Package `co.mindport.vrbuilder.tests` containing its own `AGENT.md`.
- `Source/Core`: primary runtime/editor implementation.
- `Source/BasicInteraction`: optional interaction layer built on top of Core.
- `Source/XRInteraction`: XR Interaction Toolkit integration layer.
- `Source/PackageManager`: automatic Unity package and layer dependency management.
- `Demo`, `Samples~`, `StreamingAssets~`: demo/sample/process assets.

## Mission
- Maintain and extend `co.mindport.vrbuilder.core` without breaking editor workflows, runtime behavior, or serialized process assets.
- Change the narrowest layer that owns the behavior.
- Keep package metadata, menu flows, package-manager automation, and sample wiring consistent.

## Non-Goals
- Do not treat sibling repositories or packages as in scope unless the task explicitly includes them.
- Do not modify vendored third-party XR sample content under `Source/XRInteraction/External` unless the task is specifically about that content.
- Do not invent build, CI, release, or upgrade workflows that are not evidenced by local files.
- Do not perform broad refactors while making a targeted fix.

## Repository Map

### Directory Structure
```text
Assets/MindPort/VR Builder/Core/
├── Demo/                         # Demo content and menu-driven demo scene loading
├── Documentation~/               # Package documentation
├── Samples~/                     # Importable Unity package samples
│   ├── Demo - Core Features/
│   └── Demo - Hands Interaction/
├── Source/
│   ├── BasicInteraction/         # Basic interaction feature layer
│   ├── Core/                     # Main VR Builder Core implementation
│   │   ├── Editor/               # Unity Editor tooling
│   │   │   ├── Configuration/
│   │   │   ├── Debug/
│   │   │   ├── Input/
│   │   │   ├── ProcessAssets/
│   │   │   ├── ProcessUpgradeTool/
│   │   │   ├── ProcessValidation/
│   │   │   ├── Serialization/
│   │   │   ├── Setup/
│   │   │   ├── TextToSpeech/
│   │   │   ├── UI/
│   │   │   ├── UndoRedo/
│   │   │   ├── Unity/
│   │   │   └── XRUtils/
│   │   ├── Extensions/           # Plugin and native integration points
│   │   ├── Resources/            # Unity Resources assets
│   │   ├── Runtime/              # Runtime implementation
│   │   └── StaticAssets/         # Fonts, icons, and package assets
│   ├── PackageManager/           # Automatic dependency and layer setup
│   └── XRInteraction/            # XR Interaction Toolkit integration
│       ├── External/             # Vendored XR Hands/XRI sample content
│       │   ├── XR Hands/
│       │   └── XR Interaction Toolkit/
│       ├── PackageManager/
│       ├── Source/
│       └── StaticAssets/
└── StreamingAssets~/             # Packaged process assets
```

### Assembly Boundaries
- `VRBuilder.Core`: runtime core. Depends on `Unity.InputSystem` and `Unity.Localization`, gated by `NEWTONSOFT_JSON` and `UNITY_LOCALIZATION`.
- `VRBuilder.Core.Editor`: editor-only tooling. References Core, PackageManager, XR management/OpenXR/localization packages, and demo editor assembly.
- `VRBuilder.BasicInteraction`: optional runtime layer. Depends on Core and is gated by `VR_BUILDER`.
- `VRBuilder.BasicInteraction.Editor`: editor tooling for BasicInteraction.
- `VRBuilder.XRInteraction`: optional XR runtime layer. Depends on Core, BasicInteraction, XRI, Input System, XR Core Utils, and a starter assets assembly. Gated by `XR_INTERACTION_TOOLKIT` and `VR_BUILDER_ENABLE_XR_INTERACTION`.
- `VRBuilder.XRInteraction.Editor`: editor tooling for XR interaction.
- `VRBuilder.PackageManager.Editor`: package/dependency automation used by editor startup.

## Tech Stack
- Unity package layout
- C#
- Unity Editor and Runtime assembly split
- Unity Package Manager dependency automation
- Unity XR Management / OpenXR / XR Interaction Toolkit integration
- Newtonsoft JSON, Unity Localization, Unity Input System

## Setup and Commands

### Unity Operations
- Use Unity menu entry `Tools/VR Builder/Project Setup Wizard...` or `Window/VR Builder/Project Setup Wizard` for first-project setup.
- Use `Tools/VR Builder/Scene Setup Wizard...` or `Window/VR Builder/Scene Setup Wizard` for process/scene setup.
- Use `Tools/VR Builder/Example Scenes/Basics` and `Tools/VR Builder/Example Scenes/Hands Interaction Demo` for demo scene access.
- Expect package dependencies to be gathered automatically on editor load via `DependencyManager` and `PackageOperationsManager`.

### Formatting
To repair whitespace or style issues, run:

```bash
dotnet format whitespace . --folder
dotnet format style
```

### Repository-Specific Automation
- Editor startup can automatically install missing Unity packages.
- Editor startup can automatically set or remove scripting define symbols such as `VR_BUILDER`, `VR_BUILDER_ENABLE_XR_INTERACTION`, and related integration symbols.
- On Android, startup code can create or update `Assets/csc.rsp` with required compression assembly references.
- Pre-build TTS processing can generate audio before build when `GenerateAudioInBuildingProcess` is enabled.

### Commit and Review Expectations
- Use clear commit messages using Conventional Commits specification
- Include a short test/verification note describing what was checked.
- Call out any known limitations or follow-up tasks in the PR description.

## Operating Procedure
- Read the target area before editing; this package has multiple feature layers and auto-configuration paths.
- Default to `Source/Core` unless the change clearly belongs in `BasicInteraction`, `XRInteraction`, or `PackageManager`.
- Prefer source changes over demo/sample changes.
- Preserve menu paths, wizard flows, sample import behavior, and package-manager automation unless the task explicitly changes them.
- If changing sample or demo naming, keep `package.json` sample metadata and `Demo/Editor/DemoSceneLoader.cs` aligned.
- If touching package dependency behavior, inspect both the dependency enabler and the consuming asmdefs/version defines.
- Mark missing evidence as unverified instead of guessing.

## Code Change Rules

### Scope
- Edit only the files necessary for the task.
- Do not mix runtime, editor, sample, and vendored changes in one refactor unless the task requires coordinated updates.
- Do not move assets or source across package areas without a concrete reason.

### Code Style
- Follow `.editorconfig`: UTF-8, CRLF, spaces, indent size 4, trim trailing whitespace, final newline.
- In C#, keep `System.*` usings first and do not separate using groups.
- Match local naming and structure in the touched subsystem.

#### Comments
- Write comments that explain why.
- Avoid comments that restate the code.
- Use XML documentation for classes and public methods unless the file clearly follows a different convention.

#### Formatting
- Prefer targeted formatting over repo-wide reformatting.
- Do not normalize line endings away from CRLF in touched files.

### Error Handling
- Preserve the current pattern in the touched subsystem.
- In editor automation and dependency code, prefer existing dialog/log/progress-bar patterns over introducing new control flow.
- Do not convert compatibility warnings into hard failures unless the task explicitly requires stricter behavior.

### Unity Asset Rules
- Do not edit `.meta` files unless the change requires asset moves, renames, or GUID-preserving asset creation.
- Treat `Resources`, `StaticAssets`, `StreamingAssets~`, `Demo`, and `Samples~` as compatibility-sensitive.
- Do not rename sample folders, sample display names, or demo scenes without updating the package metadata and loader code that references them.

### Dependency and Version Rules
- Do not bump Unity package versions piecemeal.
- Keep package enabler versions, asmdef version defines, and vendored external asset versions aligned when changing XR-related dependencies.
- Be aware of existing version drift in the repository:
  - `package.json` declares Unity `2022.3`.
  - `README.md` says Unity 6 or later.
  - `VRBuilder.XRInteraction.asmdef` uses XRI version define `3.1.2`, while `XRInteractionPackageEnabler` requests `3.2.1`.
  - XR Hands vendored content is under `1.5.1`, while `XRHandsPackageEnabler` requests `1.6.1`.
- Treat these mismatches as evidence that compatibility is branch/version-sensitive. Do not "clean them up" without an explicit task.

## Safety and Approval Boundaries
- Do not modify sibling repositories/packages unless the task explicitly includes them.
- Do not edit `Source/XRInteraction/External` unless the task is specifically about vendored XR sample content.
- Do not manually hardcode scripting define symbols in unrelated places; this repository already manages symbols on editor load.
- Do not add or remove Unity packages casually; package loading is automated and tied to setup/state logic.
- Do not change layer names or indices casually; dependency enablers reserve `Teleport` at layer 31 and add `Post-Processing`.
- Do not change build-time TTS generation behavior unless the task explicitly targets build or audio generation.
- Be careful with changes that can rewrite project-wide files outside this package, especially `Assets/csc.rsp` on Android and PlayerSettings scripting defines.

## Definition of Done
- The change is implemented in the correct layer.
- Related package metadata, menu wiring, and dependency/version hooks stay consistent.
- No unintended edits were made to vendored XR content or compatibility-sensitive assets.
- Formatting matches `.editorconfig` in touched files.
- Validation actually performed is reported.
- Any remaining uncertainty, especially around Unity/XR version compatibility, is explicitly called out.

## Response Format
- Start with what changed.
- Then list validation actually performed.
- Then list risks, version-sensitive areas, or unverified assumptions.
- Reference exact file paths when discussing code.

## Important Notes

### Requirements
- Treat this as a Unity package, not a standalone app.
- Respect assembly boundaries and define constraints.
- Assume editor startup side effects exist: package fetch, symbol updates, layer setup, and first-run wizard behavior.

### Configuration
- `package.json` is authoritative for package identity, version, sample registration, and UPM metadata.
- `.editorconfig` is authoritative for formatting defaults.
- asmdefs are authoritative for assembly boundaries, package references, and compile-time gates.

### Editor Windows
- Setup flows are menu-driven and implemented in editor wizard classes.
- Demo scene access is menu-driven and falls back to the Package Manager when samples are not yet imported.

### Dependencies
- Core package automation can install `com.unity.nuget.newtonsoft-json`, `com.unity.localization`, `com.unity.inputsystem`, `com.unity.postprocessing`, `com.unity.xr.management`, `com.unity.xr.openxr`, `com.unity.xr.oculus`, and `com.unity.xr.windowsmr`.
- XR integration can install `com.unity.xr.interaction.toolkit` and `com.unity.xr.hands`.
- Dependency installation order matters because the repository uses dependency priorities and post-install hooks.
