// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

#if UNITY_XR_MANAGEMENT && OPEN_XR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
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

        protected override void InitializeXRLoader(object sender, EventArgs e)
        {
            BuilderProjectSettings settings = BuilderProjectSettings.Load();

            BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;
            FeatureHelpers.RefreshFeatures(buildTargetGroup);

            foreach (string controllerProfileType in settings.OpenXRControllerProfiles)
            {
                OpenXRFeature feature = OpenXRSettings.Instance.GetFeatures<OpenXRInteractionFeature>().FirstOrDefault(f => f.GetType().Name == controllerProfileType);

                if (feature != null)
                {
                    feature.enabled = true;
                }
            }

            if (settings.UseHandTracking)
            {
                EnableBothFeatures();
            }

            AssetDatabase.SaveAssets();

            base.InitializeXRLoader(sender, e);
        }

        private static void EnableBothFeatures()
        {
            EnsureFeatureExistsAndEnabled("UnityEngine.XR.Hands.OpenXR.HandTracking, Unity.XR.Hands", BuildTargetGroup.Standalone, "Hand Tracking Subsystem");
            EnsureFeatureExistsAndEnabled("UnityEngine.XR.Hands.OpenXR.MetaHandTrackingAim, Unity.XR.Hands", BuildTargetGroup.Standalone, "Meta Hand Tracking Aim");
        }

        private static OpenXRFeature EnsureFeatureExistsAndEnabled(string type, BuildTargetGroup group, string assetName)
        {
            OpenXRSettings settings = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
            if (settings == null)
            {
                UnityEngine.Debug.LogError($"Couldn't enable {assetName} - no OpenXR settings for {group}");
                return null;
            }

            OpenXRFeature feature = settings.GetFeature(Type.GetType(type));

            if (feature == null)
            {
                feature = ScriptableObject.CreateInstance(type) as OpenXRFeature;
                feature.name = assetName;

                string settingsPath = AssetDatabase.GetAssetPath(settings);
                AssetDatabase.AddObjectToAsset(feature, settingsPath);

                SerializedObject so = new SerializedObject(settings);
                SerializedProperty featuresProp = so.FindProperty("features") ?? so.FindProperty("m_Features");
                int idx = featuresProp.arraySize;
                featuresProp.InsertArrayElementAtIndex(idx);
                featuresProp.GetArrayElementAtIndex(idx).objectReferenceValue = feature;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            if (!feature.enabled)
            {
                feature.enabled = true;
            }

            EditorUtility.SetDirty(feature);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return feature;
        }
    }
}
#endif
