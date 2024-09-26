// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using UnityEditor;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Editor.Configuration;
using VRBuilder.Editor.UI.Drawers;

namespace VRBuilder.Editor.UI.Windows
{
    /// <summary>
    /// This class draws the Step Inspector.
    /// </summary>
    internal class StepWindow : EditorWindow, IStepView
    {
        private const int border = 4;
        /// <summary>
        /// Used to decide if the window should be focused when it is opened.
        /// </summary>
        /// <remarks>
        /// In case of a recompile e.g. when entering play mode it should be false
        /// </remarks> 
        private static bool focusWindow = false;

        private IStep step;

        [SerializeField]
        private Vector2 scrollPosition;

        [SerializeField]
        private Rect stepRect;

        /// <summary>
        /// Returns the first <see cref="StepWindow"/> which is currently opened.
        /// If there is none, creates and shows <see cref="StepWindow"/>.
        /// </summary>
        public static void ShowInspector()
        {
            StepWindow window = GetInstance(); //we are not using GetInstance(true) because of an issue with duplicated step inspectors PR#89
            window.Repaint();
            if (focusWindow)
            {
                window.Focus();
            }
        }
        public static StepWindow GetInstance(bool focus = false)
        {
            focusWindow = true;
            return GetWindow<StepWindow>("Step Inspector", focus);

        }

        private void OnEnable()
        {
            focusWindow = false;
            GlobalEditorHandler.StepWindowOpened(this);
        }

        private void OnDestroy()
        {
            GlobalEditorHandler.StepWindowClosed(this);
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnFocus()
        {
            if (step?.Data == null)
            {
                return;
            }

            if (EditorConfigurator.Instance.Validation.IsAllowedToValidate())
            {
                EditorConfigurator.Instance.Validation.Validate(step.Data, GlobalEditorHandler.GetCurrentProcess());
            }
        }

        private void OnGUI()
        {
            if (step == null)
            {
                return;
            }

            IProcessDrawer drawer = DrawerLocator.GetDrawerForValue(step, typeof(Step));

            stepRect.width = position.width;

            if (stepRect.height > position.height)
            {
                stepRect.width -= GUI.skin.verticalScrollbar.fixedWidth;
            }

            scrollPosition = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), scrollPosition, stepRect, false, false);
            {
                Rect stepDrawingRect = new Rect(stepRect.position + new Vector2(border, border), stepRect.size - new Vector2(border * 2f, border * 2f));
                stepDrawingRect = drawer.Draw(stepDrawingRect, step, ModifyStep, "Step");
                stepRect = new Rect(stepDrawingRect.position - new Vector2(border, border), stepDrawingRect.size + new Vector2(border * 2f, border * 2f));
            }
            GUI.EndScrollView();
        }

        private void ModifyStep(object newStep)
        {
            step = (IStep)newStep;
            GlobalEditorHandler.CurrentStepModified(step);
        }

        public void SetStep(IStep newStep)
        {
            step = newStep;
        }

        public IStep GetStep()
        {
            return step;
        }

        public void ResetStepView()
        {
        }
    }
}
