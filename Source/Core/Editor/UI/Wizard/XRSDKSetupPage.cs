// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.Editor.Settings;
using VRBuilder.Core.Editor.XRUtils;

namespace VRBuilder.Core.Editor.UI.Wizard
{
    /// <summary>
    /// Wizard page which retrieves and loads XR SDKs.
    /// </summary>
    internal class XRSDKSetupPage : WizardPage
    {
        private enum XRLoader
        {
            OpenXR,
            None
        }

        [Serializable]
        private struct ControllerProfileOption
        {
            public string TypeName;
            public string DisplayName;

            public ControllerProfileOption(string typeName, string displayName)
            {
                TypeName = typeName;
                DisplayName = displayName;
            }
        }

        private readonly List<XRLoader> loaderOptions = new List<XRLoader>()
        {
            XRLoader.OpenXR,
            XRLoader.None,
        };

        private readonly List<string> loaderLabels = new List<string>()
        {
            "OpenXR",
            "None",
        };

        private readonly List<XRLoader> disabledLoaderOptions = new List<XRLoader>();
        private readonly List<string> disabledControllerProfileOptions = new List<string>();
        private static readonly ControllerProfileOption[] fallbackControllerProfiles =
        {
            new ControllerProfileOption("OculusTouchControllerProfile", "Oculus Touch Controller Profile"),
            new ControllerProfileOption("MetaQuestTouchPlusControllerProfile", "Meta Quest Touch Plus Controller Profile"),
            new ControllerProfileOption("MetaQuestTouchProControllerProfile", "Meta Quest Touch Pro Controller Profile"),
            new ControllerProfileOption("MicrosoftMotionControllerProfile", "Microsoft Motion Controller Profile"),
            new ControllerProfileOption("ValveIndexControllerProfile", "Valve Index Controller Profile"),
            new ControllerProfileOption("HPReverbG2ControllerProfile", "HP Reverb G2 Controller Profile"),
            new ControllerProfileOption("HTCViveControllerProfile", "HTC Vive Controller Profile"),
            new ControllerProfileOption("KHRSimpleControllerProfile", "Khronos Simple Controller Profile"),
        };

        [SerializeField]
        private XRLoader selectedLoader = XRLoader.OpenXR;

        [SerializeField]
        private List<string> selectedControllerProfiles = new List<string>();

        [SerializeField]
        private bool wasApplied = false;

        public XRSDKSetupPage() : base("XR Hardware")
        {
        }

        /// <inheritdoc/>
        public override void Draw(Rect window)
        {
            wasApplied = false;

            GUILayout.BeginArea(window);
            {
                GUILayout.Label("XR Setup", BuilderEditorStyles.Title);
                GUILayout.Label("Choose how XR should be configured for this project.", BuilderEditorStyles.Header);
                GUILayout.Label("Select whether to configure the project for tethered VR via OpenXR. Selecting 'None' will require you to install your own XR configuration before using VR Builder's XR features.", BuilderEditorStyles.Paragraph);
                GUILayout.Space(16);

                selectedLoader = BuilderGUILayout.DrawToggleGroup(selectedLoader, loaderOptions, loaderLabels, disabledLoaderOptions);

                GUILayout.Space(16);

                if (selectedLoader == XRLoader.OpenXR)
                {
                    DrawOpenXRControllerProfiles();
                }
            }
            GUILayout.EndArea();
        }

        private void DrawOpenXRControllerProfiles()
        {
            List<ControllerProfileOption> controllerProfileOptions = fallbackControllerProfiles.ToList();

            GUILayout.Label("OpenXR Controller Profiles", BuilderEditorStyles.Header);
            GUILayout.Label("Select one or more controller profiles to enable automatically after OpenXR has been loaded. You may continue without selecting any profile, but it may cause controllers not to respond.", BuilderEditorStyles.Paragraph);
            GUILayout.Space(8);

            HashSet<string> availableProfileTypes = new HashSet<string>(controllerProfileOptions.Select(option => option.TypeName));
            selectedControllerProfiles = selectedControllerProfiles
                .Where(availableProfileTypes.Contains)
                .Distinct()
                .ToList();

            List<string> profileEntries = controllerProfileOptions.Select(option => option.TypeName).ToList();
            List<string> profileLabels = controllerProfileOptions.Select(option => option.DisplayName).ToList();
            selectedControllerProfiles = BuilderGUILayout.DrawCheckBoxList(selectedControllerProfiles, profileEntries, profileLabels, disabledControllerProfileOptions).ToList();
        }

        public override void Apply()
        {
            wasApplied = true;
        }

        /// <inheritdoc/>
        public override void Skip()
        {
            ResetSettings();
        }

        /// <inheritdoc/>
        public override void Closing(bool isCompleted)
        {
            if (isCompleted && wasApplied)
            {
                if (selectedLoader != XRLoader.OpenXR)
                {
                    return;
                }

                BuilderProjectSettings settings = BuilderProjectSettings.Load();
                settings.OpenXRControllerProfiles = selectedControllerProfiles.Distinct().ToList();
                settings.Save();

                XRLoaderHelper.LoadOpenXR();
            }
        }

        private void ResetSettings()
        {
            CanProceed = true;
            selectedLoader = XRLoader.None;
            selectedControllerProfiles.Clear();
        }
    }
}
