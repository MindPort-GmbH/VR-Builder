// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Editor.TestTools;
using VRBuilder.Editor.UI.Windows;

namespace VRBuilder.Editor.Tests.ProcessWindowTests
{
    /// <summary>
    /// Base class for all process window tests.
    /// </summary>
    internal abstract class BaseProcessWindowTest : EditorImguiTest<ProcessWindow>
    {
        /// <summary>
        /// Returns all <see cref="ITransition"/>s contained in given <see cref="IStep"/>.
        /// <remarks>
        /// It also asserts that given <see cref="IStep"/> contains valid <see cref="ITransition"/>s.
        /// </remarks>
        /// </summary>
        protected static IList<ITransition> GetTransitionsFromStep(IStep step)
        {
            IList<ITransition> transitions = step.Data.Transitions.Data.Transitions;
            Assert.NotNull(transitions);
            return transitions;
        }

        /// <summary>
        /// Returns the <see cref="IProcess"/> contained in given <see cref="ProcessWindow"/>.
        /// </summary>
        protected static IProcess ExtractProcess(ProcessWindow window)
        {
            IProcess process = window.GetProcess();
            Assert.NotNull(process);
            return process;
        }

        /// <summary>
        /// Tries to access targeted <see cref="IStep"/> from given <see cref="ITransition"/>.
        /// </summary>
        /// <param name="transition"><see cref="ITransition"/> where target <see cref="IStep"/> will be extracted.</param>
        /// <param name="step">Returned value from <see cref="ITransition"/>'s TargetStep.</param>
        /// <returns>True if given <see cref="ITransition"/> contains a target <see cref="IStep"/>, otherwise, false.</returns>
        protected static bool TryToGetStepFromTransition(ITransition transition, out IStep step)
        {
            step = transition.Data.TargetStep;
            return step != null;
        }

        /// <inheritdoc />
        public override string GivenDescription => "A process window with empty process and fixed size of 1024x512 pixels.";

        /// <inheritdoc />
        protected override string AssetFolderForRecordedActions => EditorUtils.GetCoreFolder() + "/Tests/Core/Editor/ProcessWindow/Records";

        /// <inheritdoc />
        protected override ProcessWindow Given()
        {
            if (EditorUtils.IsWindowOpened<ProcessWindow>())
            {
                EditorWindow.GetWindow<ProcessWindow>().Close();
            }

            GlobalEditorHandler.SetStrategy(new EmptyTestStrategy());

            EditorUtils.ResetKeyboardElementFocus();
            ProcessWindow window = ScriptableObject.CreateInstance<ProcessWindow>();
            window.ShowUtility();
            window.position = new Rect(Vector2.zero, window.position.size);
            window.minSize = window.maxSize = new Vector2(1024f, 512f);
            window.SetProcess(new Process("Test", new Chapter("Test", null)));
            window.Focus();

            return window;
        }

        /// <inheritdoc />
        protected override void AdditionalTeardown()
        {
            if (EditorUtils.IsWindowOpened<ProcessWindow>())
            {
                EditorWindow.GetWindow<ProcessWindow>().Close();
            }

            base.AdditionalTeardown();
            GlobalEditorHandler.SetDefaultStrategy();
        }
    }
}
