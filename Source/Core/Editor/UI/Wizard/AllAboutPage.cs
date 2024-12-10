// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.ProcessAssets;
using VRBuilder.Core.Setup;
using VRBuilder.Demo.Editor;

namespace VRBuilder.Core.Editor.UI.Wizard
{
    internal class AllAboutPage : WizardPage
    {
        [SerializeField]
        private bool loadDemoScene = true;

        public AllAboutPage() : base("Help & Documentation", false, false)
        {

        }

        public override void Draw(Rect window)
        {
            GUILayout.BeginArea(window);
            GUILayout.Label("Help & Documentation", BuilderEditorStyles.Title);
            GUILayout.Label("Have a look at the following resources for further information.", BuilderEditorStyles.Paragraph);
            GUILayout.Label("Help", BuilderEditorStyles.Header);

            BuilderGUILayout.DrawLink("Documentation", "https://mindport-gmbh.github.io/VR-Builder-Documentation/index.html", BuilderEditorStyles.IndentLarge);
            BuilderGUILayout.DrawLink("Tutorials", "https://www.mindport.co/vr-builder/tutorials", BuilderEditorStyles.IndentLarge);
            BuilderGUILayout.DrawLink("Roadmap", "https://www.mindport.co/vr-builder/roadmap", BuilderEditorStyles.IndentLarge);

            GUILayout.Label("Community", BuilderEditorStyles.Header);

            BuilderGUILayout.DrawLink("Community", "http://community.mindport.co", BuilderEditorStyles.IndentLarge);
            BuilderGUILayout.DrawLink("Contact us", "mailto:info@mindport.co", BuilderEditorStyles.IndentLarge);
            BuilderGUILayout.DrawLink("GitHub", "https://github.com/MindPort-GmbH", BuilderEditorStyles.IndentLarge);

            GUILayout.Label("Review", BuilderEditorStyles.Header);
            GUILayout.Label("If you like what we are doing, you can help us greatly by leaving a positive review on the Unity Asset Store!", BuilderEditorStyles.Paragraph);

            BuilderGUILayout.DrawLink("Leave a review", "https://assetstore.unity.com/packages/tools/visual-scripting/vr-builder-open-source-toolkit-for-vr-creation-201913#reviews", BuilderEditorStyles.IndentLarge);

            GUILayout.Label("Demo Scene", BuilderEditorStyles.Title);

            loadDemoScene = GUILayout.Toggle(loadDemoScene, "Load the demo scene after closing the wizard.", BuilderEditorStyles.Toggle);

            if (loadDemoScene)
            {
                EditorGUILayout.HelpBox("VR Builder will automatically copy the process JSON to the StreamingAssets folder before opening the demo scene.", MessageType.Info);
            }

            GUILayout.Space(16);
            GUILayout.Label("You can access the menu under Tools > VR Builder to load a demo scene at any time or create a new VR Builder scene.", BuilderEditorStyles.Paragraph);
            GUILayout.EndArea();
        }

        public override void Closing(bool isCompleted)
        {
            base.Closing(isCompleted);

            if (loadDemoScene)
            {
                DemoSceneLoader.LoadDemoScene();
                ConfigureTeleportationLayers();

                GlobalEditorHandler.SetCurrentProcess(ProcessAssetUtils.GetProcessNameFromPath(RuntimeConfigurator.Instance.GetSelectedProcess()));
                GlobalEditorHandler.StartEditingProcess();
            }
        }

        private void ConfigureTeleportationLayers()
        {
            foreach (GameObject configuratorGameObject in GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).
                Where(go => go.GetComponent<ILayerConfigurator>() != null))
            {
                ILayerConfigurator configurator = configuratorGameObject.GetComponent<ILayerConfigurator>();
                if (configurator.LayerSet == LayerSet.Teleportation)
                {
                    configurator.ConfigureLayers("Teleport", "Teleport");
                    EditorUtility.SetDirty(configuratorGameObject);
                }
            }

            EditorSceneManager.SaveOpenScenes();
        }
    }
}
