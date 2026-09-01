// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Core.Utils.Logging;

namespace VRBuilder.Core.Editor.UI.ProjectSettings
{
    internal class LoggingSettingsSection : IProjectSettingsSection
    {
        public string Title { get; } = "Process LifeCycle Logging";
        public Type TargetPageProvider { get; } = typeof(LoggingSettingsProvider);
        public int Priority { get; } = 1000;

        public void OnGUI(string searchContext)
        {
            LifeCycleLoggingConfig config = ServiceRegistry.Get<IRuntimeService>().LifeCycleLogging as LifeCycleLoggingConfig;

            EditorGUI.BeginChangeCheck();

            config.LogChapters = GUILayout.Toggle(config.LogChapters, "Log Chapter output", BuilderEditorStyles.Toggle);
            config.LogSteps = GUILayout.Toggle(config.LogSteps, "Log Step output", BuilderEditorStyles.Toggle);
            config.LogBehaviors = GUILayout.Toggle(config.LogBehaviors, "Log Behaviors output", BuilderEditorStyles.Toggle);
            config.LogTransitions = GUILayout.Toggle(config.LogTransitions, "Log Transition output", BuilderEditorStyles.Toggle);
            config.LogConditions = GUILayout.Toggle(config.LogConditions, "Log Condition output", BuilderEditorStyles.Toggle);
            config.LogDataPropertyChanges = GUILayout.Toggle(config.LogDataPropertyChanges, "Log Data Property changes", BuilderEditorStyles.Toggle);
            config.LogLockState = GUILayout.Toggle(config.LogLockState, "Log Lock/Unlock requests", BuilderEditorStyles.Toggle);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(config);
            }
        }

        ~LoggingSettingsSection()
        {
            LifeCycleLoggingConfig config = ServiceRegistry.Get<IRuntimeService>().LifeCycleLogging as LifeCycleLoggingConfig;

            if (EditorUtility.IsDirty(config))
            {
                config.Save();
            }
        }
    }
}
