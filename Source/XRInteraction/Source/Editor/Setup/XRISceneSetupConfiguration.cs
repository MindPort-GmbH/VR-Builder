using System.Collections.Generic;
using VRBuilder.Core.Editor.Setup;
using VRBuilder.XRInteraction.Configuration;

namespace VRBuilder.XRInteraction.Editor.Setup
{
    /// <summary>
    /// Abstract base class for XR Interaction Toolkit based scene setup configurations.
    /// </summary>
    public abstract class XRISceneSetupConfiguration : ISceneSetupConfiguration
    {
        protected readonly Dictionary<string, SceneSetupParameter> customSettings = new Dictionary<string, SceneSetupParameter>();

        /// <inheritdoc/>
        public abstract int Priority { get; }

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public abstract string Description { get; }

        /// <inheritdoc/>
        public abstract string SceneTemplatePath { get; }

        /// <inheritdoc/>
        public abstract string DefaultProcessController { get; }

        /// <inheritdoc/>
        public abstract string DefaultConfettiPrefab { get; }

        /// <inheritdoc/>
        public abstract string RuntimeConfigurationName { get; }

        /// <inheritdoc/>
        public abstract IEnumerable<string> AllowedExtensionAssemblies { get; }

        /// <inheritdoc/>
        public abstract string ParentObjectsHierarchy { get; }

        /// <inheritdoc/>
        public virtual Dictionary<string, SceneSetupParameter> CustomSettings => customSettings;

        /// <inheritdoc/>
        public abstract IEnumerable<string> GetSetupNames();

        public XRISceneSetupConfiguration()
        {
            customSettings.Add(XRInteractionComponentConfiguration.UseHandTrackingKey, new SceneSetupParameter(
                "Use hand tracking (OpenXR)",
                typeof(bool),
                false,
                "If enabled, a rig supporting hand tracking will be added to the scene and OpenXR will be configured accordingly. This option is disabled if the OpenXR package is not present.",
                IsOpenXRMissing,
                HandTrackingChangedCallback));

            customSettings.Add(XRInteractionComponentConfiguration.UseSpatialKeyboardKey, new SceneSetupParameter(
                "Add XR spatial keyboard",
                typeof(bool),
                true,
                "If enabled, the XRI Spatial Keyboard sample's 'XRI Global Keyboard Manager' is added to the scene so any UIToolkit text field can use the in-VR keyboard. For multi-user scenes the connection window also gets the keyboard backend/bridge components attached automatically. Greyed out when the XRI Spatial Keyboard sample is not imported.",
                IsSpatialKeyboardSampleMissing,
                null));
        }

        private static void HandTrackingChangedCallback(object newValue)
        {
            bool useHandTracking = (bool)newValue;

            if (useHandTracking)
            {
                EnableOpenXRHandSettings.FixIssues();
            }
        }

        private static bool IsOpenXRMissing()
        {
#if OPENXR_AVAILABLE
            return false;
#else
            return true;
#endif
        }

        private static bool IsSpatialKeyboardSampleMissing()
        {
#if VR_BUILDER_SPATIAL_KEYBOARD_SAMPLE
            return false;
#else
            return true;
#endif
        }
    }
}