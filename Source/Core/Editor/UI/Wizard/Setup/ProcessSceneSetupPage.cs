// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021 MindPort GmbH

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRBuilder.Editor.Setup;

namespace VRBuilder.Editor.UI.Wizard
{
    /// <summary>
    /// Wizard page which handles the process scene setup.
    /// </summary>
    internal class ProcessSceneSetupPage : WizardPage
    {
        private const int MaxProcessNameLength = 40;
        private const int MinHeightOfInfoText = 30;

        [SerializeField]
        private bool useCurrentScene = true;

        [SerializeField]
        private bool createNewScene = false;

        [SerializeField]
        private bool loadSampleScene = false;

        [SerializeField]
        private bool loadDemoScene = false;

        [SerializeField]
        private string processName = "My VR Process";

        [SerializeField]
        private string lastCreatedProcess = null;

        private readonly GUIContent infoContent;
        private readonly GUIContent warningContent;

        public ProcessSceneSetupPage() : base("Setup Process")
        {
            infoContent = EditorGUIUtility.IconContent("console.infoicon.inactive.sml");
            warningContent = EditorGUIUtility.IconContent("console.warnicon.sml");
        }

        /// <inheritdoc />
        public override void Draw(Rect window)
        {
            GUILayout.BeginArea(window);

            GUILayout.Label("Setup Process", BuilderEditorStyles.Title);

            GUI.enabled = loadSampleScene == false;
            GUILayout.Label("Name of your VR Process", BuilderEditorStyles.Header);
            processName = BuilderGUILayout.DrawTextField(processName, MaxProcessNameLength, GUILayout.Width(window.width * 0.7f));
            GUI.enabled = true;

            if (ProcessAssetUtils.CanCreate(processName, out string errorMessage) == false && lastCreatedProcess != processName)
            {
                GUIContent processWarningContent = warningContent;
                processWarningContent.text = errorMessage;
                GUILayout.Label(processWarningContent, BuilderEditorStyles.Label, GUILayout.MinHeight(MinHeightOfInfoText));
                CanProceed = false;
            }
            else
            {
                GUILayout.Space(MinHeightOfInfoText + BuilderEditorStyles.BaseIndent);
                CanProceed = true;
            }

            GUILayout.BeginHorizontal();
                GUILayout.Space(BuilderEditorStyles.Indent);
                GUILayout.BeginVertical();
                bool isUseCurrentScene = GUILayout.Toggle(useCurrentScene, "Take my current scene", BuilderEditorStyles.RadioButton);
                if (useCurrentScene == false && isUseCurrentScene)
                {
                    useCurrentScene = true;
                    createNewScene = false;
                    loadSampleScene = false;
                    loadDemoScene = false;
                }

                bool isCreateNewScene = GUILayout.Toggle(createNewScene, "Create a new scene", BuilderEditorStyles.RadioButton);
                if (createNewScene == false && isCreateNewScene)
                {
                    createNewScene = true;
                    useCurrentScene = false;
                    loadSampleScene = false;
                    loadDemoScene = false;
            }

                if(EditorReflectionUtils.AssemblyExists("VRBuilder.Editor.DemoScene"))
                {
                    loadDemoScene = GUILayout.Toggle(loadDemoScene, "Load Demo Scene", BuilderEditorStyles.RadioButton);
                    if(loadDemoScene)
                    {
                        createNewScene = false;
                        useCurrentScene = false;
                        loadSampleScene = false;
                        CanProceed = true;
                    }
                }
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (createNewScene)
            {
                GUIContent helpContent;
                string sceneInfoText = "Scene will have the same name as the process.";
                if (SceneSetupUtils.SceneExists(processName))
                {
                    sceneInfoText += " Scene already exists";
                    CanProceed = false;
                    helpContent = warningContent;
                }
                else
                {
                    helpContent = infoContent;
                }

                helpContent.text = sceneInfoText;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(BuilderEditorStyles.Indent);
                    EditorGUILayout.LabelField(helpContent, BuilderEditorStyles.Label, GUILayout.MinHeight(MinHeightOfInfoText));
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }

        /// <inheritdoc />
        public override void Apply()
        {
            if (processName == lastCreatedProcess)
            {
                return;
            }

            if (loadSampleScene)
            {
                SceneSetupUtils.CreateNewSimpleExampleScene();
                return;
            }

            if (loadDemoScene)
            {

                Assembly sampleSceneAssembly = AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == "VRBuilder.Editor.DemoScene");
                Type sceneLoaderClass = sampleSceneAssembly.GetType("VRBuilder.Editor.DemoScene.DemoSceneLoader");
                MethodInfo loadScene = sceneLoaderClass.GetMethod("LoadDemoScene");
                loadScene.Invoke(null, new object[0]);
                return;
            }

            if (useCurrentScene == false)
            {
                SceneSetupUtils.CreateNewScene(processName);
            }

            SceneSetupUtils.SetupSceneAndProcess(processName);
            lastCreatedProcess = processName;
            EditorWindow.FocusWindowIfItsOpen<WizardWindow>();
        }
    }
}
