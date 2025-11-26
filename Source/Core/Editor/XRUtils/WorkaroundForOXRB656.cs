#if UNITY_XR_MANAGEMENT && OPEN_XR_1_16
using UnityEngine.XR.OpenXR.Features;
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;

namespace VRBuilder.Core.Editor.XRUtils
{
    // TODO remove this once the issue is resolved by Unity
    /// <summary>
    /// Workaround for OpenXR issue OXRB-656 where XR controllers are inverted by Y axis when using Meta Quest 3 with OpenXR plugin.
    /// </summary>
    /// <remarks>
    /// When removing this also remove the enabling code in <see cref="OpenXRPackageEnabler"/>.
    /// </remarks>
    [OpenXRFeature(
        TargetOpenXRApiVersion = "1.1.53",
        UiName = "Workaround for issue OXRB-656",
        BuildTargetGroups = new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone },
        DocumentationLink = "https://issuetracker.unity3d.com/issues/xr-interaction-toolkit-xr-controllers-are-inverted-by-y-axis-when-using-meta-quest-3-with-openxr-plugin",
        FeatureId = featureId
    )]
    public class WorkaroundForOXRB656 : OpenXRFeature
    {
        /// <summary>
        /// The feature ID for this workaround.
        /// </summary>
        public const string featureId = "com.vrbuilder.openxr.workaround.oxrb656";
    }
}
#endif
