// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Behaviors;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.BehaviorDrawers
{
    /// <summary>
    /// Maps the bit-flags <see cref="BehaviorExecutionStages"/> to a simple
    /// three-option popup matching the legacy IMGUI drawer.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(BehaviorExecutionStages))]
    internal class BehaviorExecutionStagesElementDrawer : ElementDrawer
    {
        private enum Choice
        {
            BeforeStepExecution = 1 << 0,
            AfterStepExecution = 1 << 1,
            BeforeAndAfterStepExecution = ~0
        }

        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            BehaviorExecutionStages currentRaw = value is BehaviorExecutionStages s ? s : BehaviorExecutionStages.Activation;
            Choice currentChoice = (Choice)(int)currentRaw;

            EnumField field = new EnumField(label?.text, currentChoice)
            {
                tooltip = label?.tooltip
            };
            field.AddToClassList("vrb-field");
            field.AddToClassList("vrb-field--behavior-stages");

            field.RegisterCallback<ChangeEvent<Enum>>(evt =>
            {
                if (evt.newValue is not Choice picked) return;
                if (picked == currentChoice) return;

                BehaviorExecutionStages mapped = picked switch
                {
                    Choice.AfterStepExecution => BehaviorExecutionStages.Deactivation,
                    Choice.BeforeAndAfterStepExecution => BehaviorExecutionStages.ActivationAndDeactivation,
                    _ => BehaviorExecutionStages.Activation,
                };

                BehaviorExecutionStages oldRaw = currentRaw;
                ChangeValue(
                    getNewValueCallback: () => mapped,
                    getOldValueCallback: () => oldRaw,
                    assignValueCallback: changeCallback);

                currentRaw = mapped;
                currentChoice = picked;
            });

            return field;
        }
    }
}
