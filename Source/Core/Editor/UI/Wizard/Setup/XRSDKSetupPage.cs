// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021 MindPort GmbH

using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using VRBuilder.Editor.XRUtils;
using VRBuilder.Editor.Analytics;
using UnityEditor;

namespace VRBuilder.Editor.UI.Wizard
{
    /// <summary>
    /// Wizard page which retrieves and loads XR SDKs.
    /// </summary>
    internal class XRSDKSetupPage : WizardPage
    {
        private enum XRLoader
        {
            None,
            OpenXR,
            Oculus,
            WindowsMR,
            Other
        }

        private readonly List<XRLoader> options = new List<XRLoader>(Enum.GetValues(typeof(XRLoader)).Cast<XRLoader>());

        private readonly List<string> nameplates = new List<string>()
        {
            "None",
            "OpenXR",
            "Oculus Quest/Rift",
            "Windows MR",
            "Other"
        };

        private readonly List<XRLoader> disabledOptions = new List<XRLoader>();

        [SerializeField]
        private XRLoader selectedLoader = XRLoader.None;

        [SerializeField]
        private string otherHardwareText = null;

        [SerializeField]
        private bool wasApplied = false;

        public XRSDKSetupPage() : base("XR Hardware")
        {
#if !UNITY_2020_1_OR_NEWER
            disabledOptions.Add(XRLoader.OpenXR);
#endif            
        }

        /// <inheritdoc/>
        public override void Draw(Rect window)
        {
            wasApplied = false;

            GUILayout.BeginArea(window);
            {
                GUILayout.Label("VR Hardware Setup", BuilderEditorStyles.Title);
                GUILayout.Label("Select the VR hardware you are working with:", BuilderEditorStyles.Header);
                selectedLoader = BuilderGUILayout.DrawToggleGroup(selectedLoader, options, nameplates, disabledOptions);

                if (selectedLoader == XRLoader.Other)
                {
                    GUILayout.Label("VR Builder does not provide an automated setup for your device. You need to refer to your device's vendor documentation in order to enable a compatible loader in the Unity's XR Plugin Management.", BuilderEditorStyles.Paragraph);
                }
            }
            GUILayout.EndArea();
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
                switch (selectedLoader)
                {
                    case XRLoader.Oculus:
                        XRLoaderHelper.LoadOculus();
                        break;
                    case XRLoader.OpenXR:
                        XRLoaderHelper.LoadOpenXR();
                        break;
                    case XRLoader.WindowsMR:
                        XRLoaderHelper.LoadWindowsMR();
                        break;
                }
            }
        }

        private void ResetSettings()
        {
            CanProceed = false;
            selectedLoader = XRLoader.None;
            otherHardwareText = string.Empty;
        }
    }
}
