using System;
using System.Collections.Generic;
using VRBuilder.Core.Configuration;
using VRBuilder.ProcessController;

namespace VRBuilder.Core.Editor.Setup
{
    /// <summary>
    /// Default configuration.
    /// </summary>
    public class DefaultSceneSetupConfiguration : ISceneSetupConfiguration
    {
        /// <inheritdoc/>
        public int Priority => 64;

        /// <inheritdoc/>
        public string Name => "Single user - Default";

        /// <inheritdoc/>
        public string DefaultProcessController => typeof(StandardProcessController).AssemblyQualifiedName;

        /// <inheritdoc/>
        public string RuntimeConfigurationName => typeof(DefaultRuntimeConfiguration).AssemblyQualifiedName;

        /// <inheritdoc/>
        public string Description => "The default VR Builder scene. This configuration includes the default rig for the active interaction component, " +
            "a single VR Builder process and the default process controller.";

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
                "VRBuilder.BasicInteraction.Editor.Setup.DefaultRigSceneSetup",
                "VRBuilder.Core.Editor.Setup.ProcessControllerSceneSetup",
            };
        }
    }
}
