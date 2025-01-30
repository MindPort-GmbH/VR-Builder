using System;
using System.Collections.Generic;
using VRBuilder.Core.Configuration;
using VRBuilder.ProcessController;

namespace VRBuilder.Core.Editor.Setup
{
    /// <summary>
    /// Basic configuration with rig loader.
    /// </summary>
    public class RigLoaderSceneSetupConfiguration : ISceneSetupConfiguration
    {
        /// <inheritdoc/>
        public int Priority => 128;

        /// <inheritdoc/>
        public string Name => "Single user - Rig loader";

        /// <inheritdoc/>
        public string DefaultProcessController => typeof(StandardProcessController).AssemblyQualifiedName;

        /// <inheritdoc/>
        public string RuntimeConfigurationName => typeof(DefaultRuntimeConfiguration).AssemblyQualifiedName;

        /// <inheritdoc/>
        public string Description => "Similar to the default configuration, except there is no rig in the editor scene. The rig is spawned at runtime by " +
            "the INTERACTION_RIG_LOADER object at the DUMMY_USER position. This can be useful for advanced use cases requiring to switch rig at runtime, " +
            "but it makes it harder to customize the rig.";

        /// <inheritdoc/>
        public IEnumerable<string> AllowedExtensionAssemblies => Array.Empty<string>();

        /// <inheritdoc/>
        public string DefaultConfettiPrefab => "Confetti/Prefabs/MindPortConfettiMachine";

        /// <inheritdoc/>
        public string ParentObjectsHierarchy => "";

        /// <inheritdoc/>
        public string SceneTemplatePath => "";

        /// <inheritdoc/>
        public IEnumerable<string> GetSetupNames()
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
