// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.UI.Drawers;
using VRBuilder.Core.Editor.UI.GraphView;

namespace VRBuilder.Core.Editor.UI.Windows
{
    /// <summary>
    /// This class draws the Step Inspector.
    /// </summary>
    public class StepWindow : EditorWindow, IStepView
    {
        /// <summary>
        /// Static flag to track domain reloads like enter play mode or recompile.
        /// </summary>
        /// <remarks>
        /// A static variable is reset after each domain reload.
        /// </remarks> 
        private static bool domainReload = true;

        private const int border = 4;
        private const double maxRepaintRateHz = 15d;

        private IStep step;
        private bool isDirty = true;
        private double lastRepaintTimestamp;
        private bool isStepModifiedUpdateScheduled;
        private readonly HashSet<IStep> pendingModifiedSteps = new HashSet<IStep>();

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
            //we are not using GetInstance(true) because of an issue with duplicated step inspectors PR#89
            StepWindow window = GetInstance();
            window.MarkDirty();
            window.Repaint();

            if (domainReload)
            {
                domainReload = false;
            }
            else
            {
                // Only focus on user interactions but not on domain reloads.
                window.Focus();
            }
        }
        public static StepWindow GetInstance(bool focus = false)
        {
            return GetWindow<StepWindow>("Step Inspector", focus);
        }

        private void OnEnable()
        {
            wantsLessLayoutEvents = true;
            Undo.undoRedoPerformed += MarkDirty;
            EditorApplication.projectChanged += MarkDirty;
            EditorApplication.hierarchyChanged += MarkDirty;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            GlobalEditorHandler.StepWindowOpened(this);
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= MarkDirty;
            EditorApplication.projectChanged -= MarkDirty;
            EditorApplication.hierarchyChanged -= MarkDirty;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.delayCall -= FlushStepModified;
            isStepModifiedUpdateScheduled = false;
            pendingModifiedSteps.Clear();
        }

        private void OnDestroy()
        {
            GlobalEditorHandler.StepWindowClosed(this);
        }

        private void OnInspectorUpdate()
        {
            if (isDirty == false)
            {
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            if (now - lastRepaintTimestamp < 1d / maxRepaintRateHz)
            {
                return;
            }

            lastRepaintTimestamp = now;
            isDirty = false;
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
                MarkDirty();
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
            MarkDirty();
            ScheduleStepModifiedUpdate(step);
        }

        public void SetStep(IStep newStep)
        {
            step = newStep;
            MarkDirty();
        }

        public IStep GetStep()
        {
            return step;
        }

        public void ResetStepView()
        {
        }

        private void MarkDirty()
        {
            isDirty = true;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            MarkDirty();
        }

        private void ScheduleStepModifiedUpdate(IStep modifiedStep)
        {
            if (modifiedStep == null)
            {
                return;
            }

            pendingModifiedSteps.Add(modifiedStep);

            if (isStepModifiedUpdateScheduled)
            {
                return;
            }

            isStepModifiedUpdateScheduled = true;
            EditorApplication.delayCall += FlushStepModified;
        }

        private void FlushStepModified()
        {
            EditorApplication.delayCall -= FlushStepModified;
            isStepModifiedUpdateScheduled = false;
            if (pendingModifiedSteps.Count == 0)
            {
                return;
            }

            List<IStep> stepsToNotify = new List<IStep>(pendingModifiedSteps);
            pendingModifiedSteps.Clear();

            foreach (IStep modifiedStep in stepsToNotify)
            {
                if (modifiedStep != null)
                {
                    GlobalEditorHandler.CurrentStepModified(modifiedStep);
                }
            }
        }
    }
}
