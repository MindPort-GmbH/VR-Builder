// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

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
            OpenXR_OculusTouch_Tethered,
            OpenXR_QuestPro_Tethered,
            OpenXR_ValveIndex_Tethered,
            OpenXR_HtcVive_Tethered,
            OpenXR_ReverbG2_Tethered,
            OpenXR_WMR_Tethered,
            Oculus_Tethered,
            OpenXR_Tethered,
        }

        private readonly List<XRLoader> options = new List<XRLoader>(Enum.GetValues(typeof(XRLoader)).Cast<XRLoader>());

        private readonly List<string> nameplates = new List<string>()
        {
            "Meta Quest / Pico Neo 3",
            "Meta Quest Pro",
            "Valve Index",
            "HTC Vive",
            "HP Reverb G2",
            "WMR Devices",
            "Meta Quest / Oculus Rift (Legacy)",
            "Other (Default OpenXR)",
        };

        private readonly XRLoader[] openXRLoaders =
        {
            XRLoader.OpenXR_ReverbG2_Tethered,
            XRLoader.OpenXR_QuestPro_Tethered,
            XRLoader.OpenXR_OculusTouch_Tethered,
            XRLoader.OpenXR_Tethered,
            XRLoader.OpenXR_HtcVive_Tethered,
            XRLoader.OpenXR_WMR_Tethered,
            XRLoader.OpenXR_ValveIndex_Tethered,
        };

        private readonly XRLoader[] oculusLoaders =
        {
            XRLoader.Oculus_Tethered,
        };

        private readonly List<XRLoader> disabledOptions = new List<XRLoader>();

        [SerializeField]
        private List<XRLoader> selectedLoaders = new List<XRLoader>();

        [SerializeField]
        private bool wasApplied = false;

        public XRSDKSetupPage() : base("XR Hardware")
        {
#if !UNITY_2020_1_OR_NEWER
            disabledOptions.AddRange(openXRLoaders);
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

                selectedLoaders = BuilderGUILayout.DrawCheckBoxList(selectedLoaders, options, nameplates, disabledOptions).ToList();

                ExcludeIncompatibleLoaders();

                GUILayout.Space(16);
                GUILayout.Label("The automated setup will configure your headset in tethered mode, which can be useful for testing your application while you are building it.\n" +
                    "If you want to build your application for a standalone headset like the Meta Quest line, additional setup is needed. You can refer to the following guides to do so.", BuilderEditorStyles.Paragraph);

                BuilderGUILayout.DrawLink("Meta Quest Setup Guide", "https://www.mindport.co/vr-builder-tutorials/oculus-quest-device-setup", BuilderEditorStyles.IndentLarge);
                BuilderGUILayout.DrawLink("Pico Setup Guide", "https://www.mindport.co/vr-builder-tutorials/pico-neo-device-setup", BuilderEditorStyles.IndentLarge);
            }
            GUILayout.EndArea();
        }

        private void ExcludeIncompatibleLoaders()
        {
            if (selectedLoaders.Any(oculusLoaders.Contains))
            {
                disabledOptions.AddRange(openXRLoaders);
            }
            else if (selectedLoaders.Any(openXRLoaders.Contains))
            {
                disabledOptions.AddRange(oculusLoaders);
            }
            else
            {
                disabledOptions.RemoveAll(oculusLoaders.Contains);
                disabledOptions.RemoveAll(openXRLoaders.Contains);
            }
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
                foreach (XRLoader loader in selectedLoaders)
                {
                    switch (loader)
                    {
                        case XRLoader.Oculus_Tethered:
                            XRLoaderHelper.LoadOculus();
                            break;
                        case XRLoader.OpenXR_Tethered:
                            XRLoaderHelper.LoadOpenXR();
                            break;
                        case XRLoader.OpenXR_WMR_Tethered:
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
