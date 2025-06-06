// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using UnityEditor;

namespace VRBuilder.Core.Editor.UI.ProjectSettings
{
    internal class LoggingSettingsProvider : BaseSettingsProvider
    {
        const string Path = "Project/VR Builder/Logging";

        public LoggingSettingsProvider() : base(Path, SettingsScope.Project)
        {
        }

        protected override void InternalDraw(string searchContext)
        {
        }

        [SettingsProvider]
        public static SettingsProvider Provider()
        {
            SettingsProvider provider = new LoggingSettingsProvider();
            return provider;
        }
    }
}
