# Step Inspector tab icons

One PNG per Step Inspector dock tab, used on both editor skins. If a file is missing the tab falls
back to text-only.

Expected filenames (loaded by `DetachedPanelWindow.IconFor`):

| Tab              | File                          |
| ---------------- | ----------------------------- |
| Step             | `step_light.png`              |
| Behaviors        | `behaviors_light.png`         |
| Transitions      | `transitions_light.png`       |
| Unlocked Objects | `unlocked-objects_light.png`  |

Recommended source size ~32×32 (Unity scales to the ~16px tab height). To change an icon later, just
overwrite the PNG and let Unity reimport — reopen the tab to pick it up.
