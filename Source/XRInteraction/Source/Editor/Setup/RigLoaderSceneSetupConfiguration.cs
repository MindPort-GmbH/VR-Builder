using System;
using System.Collections.Generic;
using VRBuilder.Core.Configuration;
using VRBuilder.ProcessController;

namespace VRBuilder.XRInteraction.Editor.Setup
{
    /// <summary>
    /// Basic configuration with rig loader.
    /// </summary>
    public class RigLoaderSceneSetupConfiguration : XRISceneSetupConfiguration
    {
        /// <inheritdoc/>
        public override int Priority => 128;

        /// <inheritdoc/>
        public override string Name => "Single user - Rig loader";

        /// <inheritdoc/>
        public override string DefaultProcessController => typeof(StandardProcessController).AssemblyQualifiedName;

        /// <inheritdoc/>
        public override string RuntimeConfigurationName => typeof(DefaultRuntimeConfiguration).AssemblyQualifiedName;

        /// <inheritdoc/>
        public override string Description => "Similar to the default configuration, except there is no rig in the editor scene. The rig is spawned at runtime by " +
            "the INTERACTION_RIG_LOADER object at the DUMMY_USER position. This can be useful for advanced use cases requiring to switch rig at runtime, " +
            "but it makes it harder to customize the rig.";

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
                "VRBuilder.Core.Editor.Setup.ProcessControllerSceneSetup",
                "VRBuilder.BasicInteraction.Editor.Setup.RigLoaderSceneSetup",
            };
        }
    }
}
