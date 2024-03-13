// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using UnityEditor;
using UnityEngine;
using VRBuilder.Editor;
using VRBuilder.Editor.TestTools;
using VRBuilder.Editor.UI.Windows;

namespace VRBuilder.Core.Tests.Editor.StepWindowTests
{
    internal abstract class BaseStepWindowTest : EditorImguiTest<StepWindow>
    {
        /// <inheritdoc />
        public override string GivenDescription => "A step inspector window with a new step with no behaviors and one transition to null with no conditions.";

        /// <inheritdoc />
        protected override string AssetFolderForRecordedActions => EditorUtils.GetCoreFolder() + "/Tests/Core/Editor/StepWindow/Records";

        /// <inheritdoc />
        protected override StepWindow Given()
        {
            if (EditorUtils.IsWindowOpened<StepWindow>())
            {
                EditorWindow.GetWindow<StepWindow>().Close();
            }

            GlobalEditorHandler.SetStrategy(new EmptyTestStrategy());

            EditorUtils.ResetKeyboardElementFocus();
            StepWindow window = ScriptableObject.CreateInstance<StepWindow>();
            window.ShowUtility();
            window.position = new Rect(Vector2.zero, window.position.size);
            window.minSize = window.maxSize = new Vector2(512f, 512f);
            window.SetStep(StepFactory.Instance.Create("Test"));
            window.Focus();

            return window;
        }

        protected override void AdditionalTeardown()
        {
            if (EditorUtils.IsWindowOpened<StepWindow>())
            {
                EditorWindow.GetWindow<StepWindow>().Close();
            }

            base.AdditionalTeardown();
            GlobalEditorHandler.SetDefaultStrategy();
        }
    }
}
