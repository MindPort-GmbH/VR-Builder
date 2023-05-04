// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using VRBuilder.Editor.UI.Windows;
using VRBuilder.Editor.TestTools;
using NUnit.Framework;
using UnityEngine;

namespace VRBuilder.Editor.Tests.ProcessWizardTests
{
    internal class CreateProcessTest : EditorImguiTest<ProcessCreationWizard>
    {
        private const string processName = "very_unique_test_name_which_you_should_never_use_1534";

        public override string GivenDescription => "Opened process wizard window.";

        public override string WhenDescription => $"Type in \"{processName}\". Click 'Create process' button.";

        public override string ThenDescription => "Process window is opened.";

        protected override string AssetFolderForRecordedActions => EditorUtils.GetCoreFolder() + "/Tests/Editor/ProcessCreationWizard/Records";

        protected override ProcessCreationWizard Given()
        {
            GlobalEditorHandler.SetStrategy(new EmptyTestStrategy());

            ProcessAssetManager.Delete(processName);

            foreach (ProcessCreationWizard window in Resources.FindObjectsOfTypeAll<ProcessCreationWizard>())
            {
                window.Close();
            }

            ProcessCreationWizard wizard = ScriptableObject.CreateInstance<ProcessCreationWizard>();
            wizard.ShowUtility();
            wizard.maxSize = wizard.minSize;
            wizard.position = new Rect(Vector2.zero, wizard.minSize);
            return wizard;
        }

        protected override void Then(ProcessCreationWizard window)
        {
            Assert.False(EditorUtils.IsWindowOpened<ProcessWindow>());
        }

        protected override void AdditionalTeardown()
        {
            base.AdditionalTeardown();
            foreach (ProcessCreationWizard window in Resources.FindObjectsOfTypeAll<ProcessCreationWizard>())
            {
                window.Close();
            }

            ProcessAssetManager.Delete(processName);
            GlobalEditorHandler.SetDefaultStrategy();
        }
    }
}
