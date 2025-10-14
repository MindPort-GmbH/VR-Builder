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
        protected Dictionary<string, SceneSetupParameter> customSettings = new Dictionary<string, SceneSetupParameter>();

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
                "Use hand tracking",
                typeof(bool),
                false,
                "If enabled, a rig supporting hand tracking will be added to the scene and OpenXR will be configured accordingly.",
                IsOpenXRAvailable,
                HandTrackingChangedCallback));
        }

        private static void HandTrackingChangedCallback(object newValue)
        {
            bool useHandTracking = (bool)newValue;

            if (useHandTracking)
            {
                EnableOpenXRHandSettings.FixIssues();
            }
        }

        private static bool IsOpenXRAvailable()
        {
#if OPENXR_AVAILABLE                       
            return false;
#else
            return true;
#endif
        }
    }
}