// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

#if UNITY_XR_MANAGEMENT && OPEN_XR
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using VRBuilder.Core.Editor.Settings;

namespace VRBuilder.Core.Editor.XRUtils
{
    /// <summary>
    /// Enables the Open XR Plugin.
    /// </summary>
    internal sealed class OpenXRPackageEnabler : XRProvider
    {
        /// <inheritdoc/>
        public override string Package { get; } = "com.unity.xr.openxr";

        /// <inheritdoc/>
        public override int Priority { get; } = 2;

        protected override string XRLoaderName { get; } = "OpenXRLoader";

        private const int WaitTimeoutMilliseconds = 30000;
        private const int WaitStepMilliseconds = 200;

        protected override async void InitializeXRLoader(object sender, EventArgs e)
        {
            BuilderProjectSettings settings = BuilderProjectSettings.Load();
            BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;

            // Wait for OpenXR to be ready
            if (!await WaitForOpenXRSettings(buildTargetGroup))
            {
                UnityEngine.Debug.LogError("[VR Builder] OpenXRSettings could not be accesed. Controller profiles are not enabled. Try manually enabling profiles in Project Settings > XR Plug-in Management > OpenXR.");
            }
            else
            {
                // Enable controller profiles
                foreach (string controllerProfileType in settings.OpenXRControllerProfiles)
                {
                    await EnableFeatureWithRetry<OpenXRInteractionFeature>(controllerProfileType, buildTargetGroup);
                }
            }

            AssetDatabase.SaveAssets();
            base.InitializeXRLoader(sender, e);
        }

        /// <summary>
        /// Waits for OpenXR Settings to be available, checking Unity's compilation state.
        /// </summary>
        private async Task<bool> WaitForOpenXRSettings(BuildTargetGroup buildTargetGroup)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (OpenXRSettings.Instance == null && stopwatch.ElapsedMilliseconds < WaitTimeoutMilliseconds)
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                {
                    await Task.Delay(WaitStepMilliseconds);
                    continue;
                }

                await Task.Delay(WaitStepMilliseconds);
            }

            return OpenXRSettings.Instance != null;
        }

        /// <summary>
        /// Attempts to find and enable an OpenXR feature with retry logic.
        /// </summary>
        private async Task EnableFeatureWithRetry<T>(string featureName, BuildTargetGroup buildTargetGroup) where T : OpenXRFeature
        {
            OpenXRFeature feature = null;
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (feature == null && stopwatch.ElapsedMilliseconds < WaitTimeoutMilliseconds)
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating || OpenXRSettings.Instance == null)
                {
                    await Task.Delay(WaitStepMilliseconds);
                    continue;
                }

                FeatureHelpers.RefreshFeatures(buildTargetGroup);
                OpenXRFeature[] features = OpenXRSettings.Instance.GetFeatures<T>();

                if (features != null && features.Any())
                {
                    feature = features.FirstOrDefault(f => f.GetType().Name == featureName);
                }

                if (feature != null)
                {
                    break;
                }

                await Task.Delay(WaitStepMilliseconds);
            }

            if (feature != null)
            {
                feature.enabled = true;
                UnityEngine.Debug.Log($"[VR Builder] Enabled OpenXR feature: {featureName}");
            }
            else
            {
                UnityEngine.Debug.LogError($"[VR Builder] Could not find OpenXR feature: {featureName}. Please enable it manually in Project Settings > XR Plug-in Management > OpenXR.");
            }
        }
    }
}
#endif
