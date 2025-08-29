using Unity.XR.CoreUtils.Editor;
using UnityEditor;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace VRBuilder.Editor.Validation
{
	/// <summary>
	/// Registers a Project Validation rule that ensures
	/// XR Hands "Hand Tracking Sybsysthem" and "Meta Hand Tracking Aim" are enabled.
	/// </summary>
	internal static class OpenXRHandsProjectValidation
	{
		private const string kCategory = "VR Builder";
		private const string kMessage =
			"[Optional] To use the hand tracking rig 'VRB_XR_Setup_Hands', enable XR Hands 'Hand Tracking Subsystem' and 'Meta Hand Tracking Aim'.";

		// Guard so we only add once per domain reload.
		private static bool s_Added;

		[InitializeOnLoadMethod]
		private static void Register()
		{
			if (s_Added)
				return;
			s_Added = true;

			BuildValidationRule[] rules = new[]
			{
				new BuildValidationRule
				{
					Category       = kCategory,
					Message        = kMessage,
                    // Only show this rule if OpenXR is the selected loader for Standalone.
                    IsRuleEnabled  = () =>
						XRPackageMetadataStore.IsLoaderAssigned(
							typeof(OpenXRLoader).FullName, BuildTargetGroup.Standalone),
                    // Passes only if both features are enabled.
                    CheckPredicate = AreBothFeaturesEnabled,
                    // Explain how to fix it manually.
                    FixItMessage   =
						"Enables <i>Hand Tracking Subsystem</i> and <i>Meta Hand Tracking Aim</i> in Project Settings > XR Plug-in Management > OpenXR (Windows/PC tab).",
                    // One-click auto-fix.
                    FixItAutomatic = true,
					FixIt          = EnableBothFeatures,
                    // We want a warning, not a build-stopping error.
                    Error          = false,
				}
			};

			BuildValidator.AddRules(BuildTargetGroup.Standalone, rules);
		}

		private static bool AreBothFeaturesEnabled()
		{
			OpenXRSettings settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Standalone);
			if (settings == null)
			{
				return false;
			}

			HandTracking hand = settings.GetFeature<HandTracking>();
			MetaHandTrackingAim metaAim = settings.GetFeature<MetaHandTrackingAim>();

			return hand != null && hand.enabled && metaAim != null && metaAim.enabled;
		}

		private static void EnableBothFeatures()
		{
			EnsureFeatureExistsAndEnabled<HandTracking>(BuildTargetGroup.Standalone, "Hand Tracking Subsystem");
			EnsureFeatureExistsAndEnabled<MetaHandTrackingAim>(BuildTargetGroup.Standalone, "Meta Hand Tracking Aim");
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
	}
}
