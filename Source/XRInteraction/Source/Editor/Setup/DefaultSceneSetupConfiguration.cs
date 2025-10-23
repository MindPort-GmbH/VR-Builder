using System;
using System.Collections.Generic;
using VRBuilder.Core.Configuration;
using VRBuilder.ProcessController;

namespace VRBuilder.XRInteraction.Editor.Setup
{
    /// <summary>
    /// Default configuration.
    /// </summary>
    public class DefaultSceneSetupConfiguration : XRISceneSetupConfiguration
    {
        /// <inheritdoc/>
        public override int Priority => 64;

        /// <inheritdoc/>
        public override string Name => "Single user - Default";

        /// <inheritdoc/>
        public override string DefaultProcessController => typeof(StandardProcessController).AssemblyQualifiedName;

        /// <inheritdoc/>
        public override string RuntimeConfigurationName => typeof(DefaultRuntimeConfiguration).AssemblyQualifiedName;

        /// <inheritdoc/>
        public override string Description => "The default VR Builder scene. This configuration includes the default rig for the active interaction component, " +
            "a single VR Builder process and the default process controller.";

        /// <inheritdoc/>
        public override IEnumerable<string> AllowedExtensionAssemblies => Array.Empty<string>();

        /// <inheritdoc/>
        public override string DefaultConfettiPrefab => "Confetti/Prefabs/MindPortConfettiMachine";

        /// <inheritdoc/>
        public override string ParentObjectsHierarchy => "";

        /// <inheritdoc/>
        public override string SceneTemplatePath => "";

        /// <inheritdoc/>
        public override IEnumerable<string> GetSetupNames()
        {
            return new string[]
            {
                "VRBuilder.Core.Editor.Setup.RuntimeConfigurationSetup",
                "VRBuilder.BasicInteraction.Editor.Setup.DefaultRigSceneSetup",
                "VRBuilder.Core.Editor.Setup.ProcessControllerSceneSetup",
            };
        }
    }
}
