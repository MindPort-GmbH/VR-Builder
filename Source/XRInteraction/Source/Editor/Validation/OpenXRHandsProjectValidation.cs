#if OPENXR_AVAILABLE
using Unity.XR.CoreUtils.Editor;
using UnityEditor;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace VRBuilder.XRInteraction.Editor.Validation
{
    /// <summary>
    /// Registers a Project Validation rule that ensures
    /// XR Hands "Hand Tracking Sybsysthem" and "Meta Hand Tracking Aim" are enabled.
    /// </summary>
    public static class OpenXRHandsProjectValidation
    {
        private const string RerunAfterOpenXRIsAvailable = "VRB.OpenXR.RerunAfterOpenXRIsAvailable";
        private const string kCategory = "VR Builder";
        private const string kMessage =
            "[Optional] To use the hand tracking rig 'VRB_XR_Setup_Hands', enable XR Hands 'Hand Tracking Subsystem' and 'Meta Hand Tracking Aim'.";
        private const string kFixItStandalone =
            "Enables <i>Hand Tracking Subsystem</i> and <i>Meta Hand Tracking Aim</i> in Project Settings > XR Plug-in Management > OpenXR (Windows/PC tab).";
        private const string kFixItAndroid =
            "Enables <i>Hand Tracking Subsystem</i> and <i>Meta Hand Tracking Aim</i> in Project Settings > XR Plug-in Management > OpenXR (Android tab).";

        // Guard so we only add once per domain reload.
        private static bool s_Added;

        private static readonly BuildValidationRule[] s_BuildValidationRulesStandalone = new[]
        {
            new BuildValidationRule
            {
                Category       = kCategory,
                Message        = kMessage,
                // Only show this rule if OpenXR is the selected loader for Standalone.
                IsRuleEnabled  = () => XRPackageMetadataStore.IsLoaderAssigned(typeof(OpenXRLoader).FullName, BuildTargetGroup.Standalone),
                // Passes only if both features are enabled.
                CheckPredicate = () => AreFeaturesEnabled(BuildTargetGroup.Standalone),
                // Explain how to fix it manually.
                FixItMessage   = kFixItStandalone,
                // One-click auto-fix.
                FixItAutomatic = true,
                FixIt          = () => EnableFeatures(BuildTargetGroup.Standalone),
                // We want a warning, not a build-stopping error.
                Error          = false,
            }
        };

        private static readonly BuildValidationRule[] s_BuildValidationRulesAndroid = new[]
        {
            new BuildValidationRule
            {
                Category       = kCategory,
                Message        = kMessage,
                // Only show this rule if OpenXR is the selected loader for Android.
                IsRuleEnabled  = () => XRPackageMetadataStore.IsLoaderAssigned(typeof(OpenXRLoader).FullName, BuildTargetGroup.Android),
                // Passes only if both features are enabled.
                CheckPredicate = () => AreFeaturesEnabled(BuildTargetGroup.Android),
                // Explain how to fix it manually.
                FixItMessage   = kFixItAndroid,
                // One-click auto-fix.
                FixItAutomatic = true,
                FixIt          = () => EnableFeatures(BuildTargetGroup.Android),
                // We want a warning, not a build-stopping error.
                Error          = false,
            }
        };

        [InitializeOnLoadMethod]
        private static void Setup()
        {
            RegisterRules();
        }

        private static void RegisterRules()
        {
            if (s_Added)
            {
                return;
            }
            s_Added = true;

            BuildValidator.AddRules(BuildTargetGroup.Standalone, s_BuildValidationRulesStandalone);
            BuildValidator.AddRules(BuildTargetGroup.Android, s_BuildValidationRulesAndroid);
        }

        private static bool AreFeaturesEnabled(BuildTargetGroup group)
        {
            OpenXRSettings settings = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
            if (settings == null)
            {
                Debug.LogError($"AreBothFeaturesEnabled: Couldn't get OpenXRSettings for {group}");
                return false;
            }

            HandTracking hand = settings.GetFeature<HandTracking>();
            MetaHandTrackingAim metaAim = settings.GetFeature<MetaHandTrackingAim>();

            return hand != null && hand.enabled && metaAim != null && metaAim.enabled;
        }

        private static void EnableFeatures(BuildTargetGroup group)
        {
            EnsureFeatureExistsAndEnabled<HandTracking>(group, "Hand Tracking Subsystem");
            EnsureFeatureExistsAndEnabled<MetaHandTrackingAim>(group, "Meta Hand Tracking Aim");
        }

        private static T EnsureFeatureExistsAndEnabled<T>(BuildTargetGroup group, string assetName) where T : OpenXRFeature
        {
            OpenXRSettings settings = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
            if (settings == null)
            {
                Debug.LogError($"Couldn't enable {assetName} - no OpenXR settings for {group}");
                return null;
            }

            T feature = settings.GetFeature<T>();

            if (feature == null)
            {
                feature = ScriptableObject.CreateInstance<T>();
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

        public static void FixAllValidationIssues()
        {
            foreach (var validation in s_BuildValidationRulesStandalone)
            {
                if (validation.CheckPredicate == null || !validation.CheckPredicate.Invoke())
                {
                    validation.FixIt?.Invoke();
                }
            }

            foreach (var validation in s_BuildValidationRulesAndroid)
            {
                if (validation.CheckPredicate == null || !validation.CheckPredicate.Invoke())
                {
                    validation.FixIt?.Invoke();
                }
            }
        }
    }
}

#endif