using System.Collections.Generic;

namespace VRBuilder.Editor.Setup
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
        /// Gets the required scene setup actions for this configuration.
        /// </summary>        
        IEnumerable<string> GetSetupNames();
    }
}
