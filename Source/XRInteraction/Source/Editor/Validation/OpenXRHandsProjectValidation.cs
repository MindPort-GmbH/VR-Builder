using UnityEditor;

#if !OPENXR_AVAILABLE
using VRBuilder.PackageManager.Editor;
#else
using Unity.XR.CoreUtils.Editor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
#endif

namespace VRBuilder.XRInteraction.Editor.Validation
{
	/// <summary>
	/// Provides validation and setup utilities for Handtracking with OpenXR.
	/// </summary>
	/// <remarks>
	/// When OpenXR is not available, FixAllValidationIssues installs the OpenXR package first and stores a flag to resume after domain reload.
	/// On reload with OpenXR available, the class resumes FixAllValidationIssues to enable OpenXR for PC and enables 
	/// Hand Tracking Subsystem and Meta Hand Tracking Aim.
	/// </remarks>
	public static class OpenXRHandsProjectValidation
	{
		private const string RerunAfterOpenXRIsAvalible = "VRB.OpenXR.RerunAfterOpenXRIsAvalible";
#if !OPENXR_AVAILABLE
		private const string openXRPackage = "com.unity.xr.openxr";
#else
		private const string kCategory = "VR Builder";
		private const string kMessage =
			"[Optional] To use the hand tracking rig 'VRB_XR_Setup_Hands', enable XR Hands 'Hand Tracking Subsystem' and 'Meta Hand Tracking Aim'.";

		// Guard so we only add once per domain reload.
		private static bool s_Added;

		private static readonly BuildValidationRule[] s_BuildValidationRules = new[]
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
#endif

#if !OPENXR_AVAILABLE
		private static void OnPackageManagerInitialized(object sender, PackageOperationsManager.InitializedEventArgs e)
		{
			PackageOperationsManager.OnInitialized -= OnPackageManagerInitialized;
			EnsureOpenXRPackage();
		}

		private static void EnsureOpenXRPackage()
		{
			PackageOperationsManager.LoadPackage(openXRPackage);
		}
#endif

#if OPENXR_AVAILABLE

		[InitializeOnLoadMethod]
		private static void Setup()
		{
			RegisterRules();

			if (EditorPrefs.GetBool(RerunAfterOpenXRIsAvalible, false))
			{
				EditorPrefs.SetBool(RerunAfterOpenXRIsAvalible, false);
				EnableOpenXRLoaderAssignedForPC();
				FixAllValidationIssues();
			}
		}

		private static void RegisterRules()
		{
			if (s_Added)
			{
				return;
			}
			s_Added = true;

			BuildValidator.AddRules(BuildTargetGroup.Standalone, s_BuildValidationRules);
		}

		/// <summary>
		/// Ensures that the OpenXR loader is assigned for PC/Standalone builds.
		/// </summary>
		/// <remarks>
		/// There is another more geniric implementation of this method in <see cref="XRLoaderHelper.TryToEnableLoader"/>.
		/// However it is using an older API setup and strings to determine the loaderType.
		/// Consider merging the two implementations in the future.
		/// </remarks>
		private static void EnableOpenXRLoaderAssignedForPC()
		{
			XRGeneralSettings generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
			if (generalSettings == null)
			{
				return;
			}

			XRManagerSettings managerSettings = generalSettings.AssignedSettings;
			if (managerSettings == null)
			{
				return;
			}

			string loaderType = typeof(OpenXRLoader).FullName;
			if (XRPackageMetadataStore.IsLoaderAssigned(loaderType, BuildTargetGroup.Standalone))
			{
				return;
			}

			if (XRPackageMetadataStore.AssignLoader(managerSettings, loaderType, BuildTargetGroup.Standalone))
			{
				EditorUtility.SetDirty(managerSettings);
				EditorUtility.SetDirty(generalSettings);
				AssetDatabase.SaveAssets();
			}
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
#endif

		public static void FixAllValidationIssues()
		{
#if !OPENXR_AVAILABLE
			EditorPrefs.SetBool(RerunAfterOpenXRIsAvalible, true);

			if (PackageOperationsManager.IsInitialized)
			{
				EnsureOpenXRPackage();
			}
			else
			{
				PackageOperationsManager.OnInitialized += OnPackageManagerInitialized;
			}
#else
			foreach (var validation in s_BuildValidationRules)
			{
				if (validation.CheckPredicate == null || !validation.CheckPredicate.Invoke())
				{
					validation.FixIt?.Invoke();
				}
			}
#endif
		}
	}
}
