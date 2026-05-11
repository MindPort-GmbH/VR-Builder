// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs
{
    /// <summary>
    /// Cross-cutting state every panel and child drawer needs while building UI.
    /// Phase 4 fills in CurrentStep / CurrentChapter / CurrentProcess via the
    /// selection service; Phase 7 adds the validation report.
    /// </summary>
    public interface IElementDrawerContext
    {
        IStep CurrentStep { get; }
        IChapter CurrentChapter { get; }
        IProcess CurrentProcess { get; }

        void NotifyStepModified();
    }
}
