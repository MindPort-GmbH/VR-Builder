// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using VRBuilder.Editor.XRUtils;

namespace VRBuilder.Editor.UI.Wizard
{
    /// <summary>
    /// Wizard page which retrieves and loads XR SDKs.
    /// </summary>
    internal class XRSDKSetupPage : WizardPage
    {
        private enum XRLoader
        {
            OpenXR_OculusTouch_Tethered,
            OpenXR_QuestPro_Tethered,
            OpenXR_ValveIndex_Tethered,
            OpenXR_HtcVive_Tethered,
            OpenXR_ReverbG2_Tethered,
            WindowsMR_Tethered,
            Oculus_Tethered,
            OpenXR_Tethered,
        }

        private readonly List<XRLoader> options = new List<XRLoader>(Enum.GetValues(typeof(XRLoader)).Cast<XRLoader>());

        private readonly List<string> nameplates = new List<string>()
        {
            "Meta Quest/Pico Neo 3",
            "Meta Quest Pro",
            "Valve Index",
            "HTC Vive",
            "HP Reverb G2",
            "WMR Devices",
            "Meta Quest/Oculus Rift (Legacy)",
            "Other (Default OpenXR)",
        };

        private readonly List<XRLoader> disabledOptions = new List<XRLoader>();

        [SerializeField]
        private IEnumerable<XRLoader> selectedLoaders = new List<XRLoader>();

        [SerializeField]
        private bool wasApplied = false;

        public XRSDKSetupPage() : base("XR Hardware")
        {
#if !UNITY_2020_1_OR_NEWER
            disabledOptions.Add(XRLoader.OpenXR);
            disabledOptions.Add(XRLoader.OpenXR_HtcVive_Tethered);
            disabledOptions.Add(XRLoader.OpenXR_OculusTouch_Tethered);
            disabledOptions.Add(XRLoader.OpenXR_ValveIndex_Tethered);
#endif            
        }

        /// <inheritdoc/>
        public override void Draw(Rect window)
        {
            wasApplied = false;

            GUILayout.BeginArea(window);
            {
                GUILayout.Label("VR Hardware Setup", BuilderEditorStyles.Title);
                GUILayout.Label("Select your VR hardware from the list below.", BuilderEditorStyles.Header);
                GUILayout.Label("VR Builder will automatically configure the project to work with your hardware in tethered mode.", BuilderEditorStyles.Paragraph);
                GUILayout.Space(16);

                selectedLoaders = BuilderGUILayout.DrawCheckBoxList(selectedLoaders, options, nameplates, disabledOptions);

                //if (selectedLoader == XRLoader.OpenXR_Tethered)
                //{
                //    GUILayout.Space(16);
                //    GUILayout.Label("You will need to enable a suitable controller profile before being able to use your hardware. Please review the OpenXR Project Settings page after setup.", BuilderEditorStyles.Paragraph);
                //}

                //if (selectedLoader == XRLoader.None)
                //{
                //    GUILayout.Space(16);
                //    GUILayout.Label("Are you using a different headset? Let us know what it is and if you would like us to provide automated setup for it! You can join our community from the Tools > VR Builder menu.", BuilderEditorStyles.Paragraph);
                //}

                GUILayout.Space(16);
                GUILayout.Label("The automated setup will configure your headset in tethered mode, which can be useful for testing your application while you are building it.\n" +
                    "If you want to build your application for a standalone headset like the Meta Quest line, additional setup is needed. You can refer to the following guides to do so.", BuilderEditorStyles.Paragraph);

                BuilderGUILayout.DrawLink("Meta Quest Setup Guide", "https://www.mindport.co/vr-builder-tutorials/oculus-quest-device-setup", BuilderEditorStyles.IndentLarge);
                BuilderGUILayout.DrawLink("Pico Setup Guide", "https://www.mindport.co/vr-builder-tutorials/pico-neo-device-setup", BuilderEditorStyles.IndentLarge);

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
                foreach(XRLoader loader in selectedLoaders)
                {
                    switch (loader)
                    {
                        case XRLoader.Oculus_Tethered:
                            XRLoaderHelper.LoadOculus();
                            break;
                        case XRLoader.OpenXR_Tethered:
                            XRLoaderHelper.LoadOpenXR();
                            break;
                        case XRLoader.WindowsMR_Tethered:
#if UNITY_2020_1_OR_NEWER
                            AddOpenXRControllerProfile("MicrosoftMotionControllerProfile");
                            XRLoaderHelper.LoadOpenXR();
#else
                        XRLoaderHelper.LoadWindowsMR();
#endif
                            break;
                        case XRLoader.OpenXR_OculusTouch_Tethered:
                            AddOpenXRControllerProfile("OculusTouchControllerProfile");
                            XRLoaderHelper.LoadOpenXR();
                            break;
                        case XRLoader.OpenXR_ValveIndex_Tethered:
                            AddOpenXRControllerProfile("ValveIndexControllerProfile");
                            XRLoaderHelper.LoadOpenXR();
                            break;
                        case XRLoader.OpenXR_HtcVive_Tethered:
                            AddOpenXRControllerProfile("HTCViveControllerProfile");
                            XRLoaderHelper.LoadOpenXR();
                            break;
                        case XRLoader.OpenXR_QuestPro_Tethered:
                            AddOpenXRControllerProfile("MetaQuestTouchProControllerProfile");
                            XRLoaderHelper.LoadOpenXR();
                            break;
                        case XRLoader.OpenXR_ReverbG2_Tethered:
                            AddOpenXRControllerProfile("HPReverbG2ControllerProfile");
                            XRLoaderHelper.LoadOpenXR();
                            break;
                    }

                }
            }
        }

        private void AddOpenXRControllerProfile(string profileType)
        {
            BuilderProjectSettings settings = BuilderProjectSettings.Load();
            settings.OpenXRControllerProfiles.Add(profileType);
            settings.Save();
        }

        private void ResetSettings()
        {
            CanProceed = false;
            selectedLoaders = new List<XRLoader>();
        }
    }
}
