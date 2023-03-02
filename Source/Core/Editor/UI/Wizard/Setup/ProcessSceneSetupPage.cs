// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Utils;
using VRBuilder.Editor.Setup;

namespace VRBuilder.Editor.UI.Wizard
{
    /// <summary>
    /// Wizard page which handles the process scene setup.
    /// </summary>
    internal class ProcessSceneSetupPage : WizardPage
    {
        [SerializeField]
        private int selectedIndex = 0;

        private readonly ISceneSetupConfiguration[] configurations;

        public ProcessSceneSetupPage() : base("Setup Process")
        {
            configurations = ReflectionUtils.GetConcreteImplementationsOf<ISceneSetupConfiguration>()
                .Select(type => ReflectionUtils.CreateInstanceOfType(type))
                .Cast<ISceneSetupConfiguration>()
                .ToArray();
        }

        /// <inheritdoc />
        public override void Draw(Rect window)
        {
            GUILayout.BeginArea(window);

            GUILayout.Label("Setup VR Builder scene", BuilderEditorStyles.Title);

            GUILayout.Label("Select a scene configuration", BuilderEditorStyles.Header);

            selectedIndex = EditorGUILayout.Popup(selectedIndex, configurations.Select(config => config.Name).ToArray());

            EditorGUILayout.HelpBox(configurations[selectedIndex].Description, MessageType.Info);

            GUILayout.EndArea();
        }

        /// <inheritdoc />
        public override void Apply()
        {
            ProcessCreationPage.Configuration = configurations[selectedIndex];
            EditorWindow.FocusWindowIfItsOpen<WizardWindow>();
        }
    }
}
