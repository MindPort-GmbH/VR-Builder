using System.Collections.Generic;
using VRBuilder.Core.Editor.Setup;
using VRBuilder.XRInteraction.Editor.Configuration;

namespace VRBuilder.XRInteraction.Editor.Setup
{
    public abstract class XRISceneSetupConfiguration : ISceneSetupConfiguration
    {
        protected Dictionary<string, SceneSetupParameter> customSettings = new Dictionary<string, SceneSetupParameter>();

        public abstract int Priority { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract string SceneTemplatePath { get; }

        public abstract string DefaultProcessController { get; }

        public abstract string DefaultConfettiPrefab { get; }

        public abstract string RuntimeConfigurationName { get; }

        public abstract IEnumerable<string> AllowedExtensionAssemblies { get; }

        public abstract string ParentObjectsHierarchy { get; }

        public virtual Dictionary<string, SceneSetupParameter> CustomSettings => customSettings;

        public abstract IEnumerable<string> GetSetupNames();

        public XRISceneSetupConfiguration()
        {
            customSettings.Add(XRInteractionComponentConfiguration.UseHandTrackingKey, new SceneSetupParameter(
                "Use hand tracking",
                typeof(bool),
                false,
                "If enabled, a rig supporting hand tracking will be added to the scene.",
                IsHandTrackingDisabled,
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

        private static bool IsHandTrackingDisabled()
        {
#if OPENXR_AVAILABLE                       
            return false;
#else
            return true;
#endif
        }
    }
}