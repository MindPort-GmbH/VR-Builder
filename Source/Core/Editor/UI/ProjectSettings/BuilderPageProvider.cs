// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using UnityEditor;

namespace VRBuilder.Core.Editor.UI.ProjectSettings
{
    internal class BuilderPageProvider : BaseSettingsProvider
    {
        const string Path = "Project/VR Builder";

        public BuilderPageProvider() : base(Path, SettingsScope.Project)
        {
        }

        protected override void InternalDraw(string searchContext)
        {
        }

        [SettingsProvider]
        public static SettingsProvider GetBuilderSettingsProvider()
        {
            SettingsProvider provider = new BuilderPageProvider();
            return provider;
        }
    }
}
