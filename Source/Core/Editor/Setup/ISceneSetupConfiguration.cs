using System.Collections.Generic;

namespace VRBuilder.Core.Editor.Setup
{
    /// <summary>
    /// Defines the configuration for a particular scene setup.
    /// </summary>
    public interface ISceneSetupConfiguration
    {
        /// <summary>
        /// Priority of this configuration.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Display name of the configuration.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Detailed description of the configuration.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Path to the template file for this scene setup. It will create a default scene if empty.
        /// </summary>
        string SceneTemplatePath { get; }

        /// <summary>
        /// Default process controller to use in this configuration.
        /// </summary>
        string DefaultProcessController { get; }

        /// <summary>
        /// Default resources prefab to use for Confetti behavior.
        /// </summary>
        string DefaultConfettiPrefab { get; }

        /// <summary>
        /// Name of the runtime configuration to use.
        /// </summary>
        string RuntimeConfigurationName { get; }

        /// <summary>
        /// Returns the names of the assemblies which contain allowed extensions.
        /// </summary>
        IEnumerable<string> AllowedExtensionAssemblies { get; }

        /// <summary>
        /// Gets the required scene setup actions for this configuration.
        /// </summary>        
        IEnumerable<string> GetSetupNames();

        /// <summary>
        /// Names of the parent objects the configuration objects should be children of, separated by '\' or '/'.
        /// </summary>
        string ParentObjectsHierarchy { get; }
    }
}
