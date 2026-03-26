# AGENTS.md
This file provides guidance to AI agents when working with code in this repository.

## Project Overview
This repository contains VR Builder Core (`co.mindport.vrbuilder.core`), a Unity package for authoring VR applications via graphical workflow editor.

If existing, use the root `AGENTS.md` for global rules (MCP usage, coding style, commit/reporting format).

## Directory Structure
```text
[Package Root]
├── Documentation~/               # Package documentation
├── Samples~/                     # Importable Unity package samples. See package.json for demo descriptions. Demos will be installed to Assets/Samples/... on user demand.
├── Source/                       # Package source code organized by feature and layer
│   ├── BasicInteraction/         # Abstract layer for XR interaction
│   ├── Core/                     # Main VR Builder Core implementation
│   │   ├── Editor/               # Editor Classes
│   │   │   ├── Configuration/    # Editor configuration defaults and settings wiring
│   │   │   ├── Debug/            # Debug windows and editor diagnostics tooling
│   │   │   ├── Input/            # Input-related editor helpers and utilities
│   │   │   ├── ProcessAssets/    # Process asset management and asset postprocessing
│   │   │   ├── ProcessUpgradeTool/ # Process migration and version upgrade tooling
│   │   │   ├── ProcessValidation/ # Process validation pipeline interfaces
│   │   │   ├── Serialization/    # Editor-side serialization for configuration data
│   │   │   ├── Setup/            # Scene and project setup workflows for editor tooling
│   │   │   ├── TextToSpeech/     # Text-to-speech configuration and conversion providers
│   │   │   ├── UI/               # Shared editor UI, styles, and inspector components
│   │   │   ├── UndoRedo/         # Command-based undo/redo infrastructure
│   │   │   ├── Unity/            # Unity editor/build integration helpers
│   │   │   └── XRUtils/          # XR package enablers and XR-specific editor utilities
│   │   ├── Extensions/           # Plugin and native integration points
│   │   ├── Resources/            # Unity Resources assets
│   │   ├── Runtime/              # Runtime implementation
│   │   └── StaticAssets/         # Fonts, icons, and package assets
│   ├── PackageManager/           # Automatic dependency and layer setup
│   └── XRInteraction/            # XR Interaction Toolkit integration
│       ├── External/             # Content copied from XRI Samples. Do not modify unless the task is specifically about that content.
│       ├── PackageManager/       # XRI package dependency enablers and setup hooks
│       ├── Source/               # XRI extension editor/runtime implementation
│       └── StaticAssets/         # XRI prefabs, materials, input actions, and static resources
└── StreamingAssets~/             # Packaged process assets
```

## Package Boundaries
- Preserve package startup automation behavior (dependency installation, symbol updates, layer setup).
- If changing package dependencies, update both:
  - dependency enabler logic, and
  - affected asmdef/version-define gates.
- Do not modify `Source/XRInteraction/External/*` unless the task explicitly targets vendored sample content.

## Authoritative Files
- `package.json`: package identity/version/sample registration.
- `*.asmdef`: assembly references and compile-time defines.

## Sibling Repositories
- VR Builder Pro is separate: `co.mindport.vrbuilder.pro` (with its own `AGENTS.md`).
- Tests are separate: `co.mindport.vrbuilder.tests` (with its own `AGENTS.md`). Implement tests there unless instructed otherwise.
- If a required sibling repository is unavailable, report once and continue with available scope.

## Validation Checklist
- Confirm the change is in the correct layer.
- Confirm related metadata/asmdef/defines remain consistent.
- Run available compile/playmode/tests checks relevant to the task and report what was actually validated.
- Explicitly list uncertainties instead of guessing.
unity.xr.hands`.
- Dependency installation order matters because the repository uses dependency priorities and post-install hooks.
