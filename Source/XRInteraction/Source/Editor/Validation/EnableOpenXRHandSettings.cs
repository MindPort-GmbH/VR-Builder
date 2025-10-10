using UnityEditor;

#if !OPENXR_AVAILABLE
using VRBuilder.PackageManager.Editor;
using VRBuilder.Core.Editor.XRUtils;
#endif

namespace VRBuilder.XRInteraction.Editor.Validation
{
	public static class EnableOpenXRHandSettings
	{
#if !OPENXR_AVAILABLE

		public static void FixIssues()
		{
			if (PackageOperationsManager.IsInitialized)
			{
				InstallOpenXRPackage();
			}
			else
			{
				PackageOperationsManager.OnInitialized += OnPackageManagerInitialized;
			}
		}

		private static void OnPackageManagerInitialized(object sender, PackageOperationsManager.InitializedEventArgs e)
		{
			PackageOperationsManager.OnInitialized -= OnPackageManagerInitialized;
			InstallOpenXRPackage();
		}

		private static void InstallOpenXRPackage()
		{
			XRLoaderHelper.LoadOpenXR();
		}
#else
		public static void FixIssues()
		{
			// Enales Hand Tracking Subsystem and Meta Hand Tracking Aim.
			OpenXRHandsProjectValidation.FixAllValidationIssues();
		}
#endif
	}
}