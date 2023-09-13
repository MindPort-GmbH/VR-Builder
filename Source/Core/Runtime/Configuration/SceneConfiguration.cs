using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Handles configuration specific to this scene.
    /// </summary>
    public class SceneConfiguration : MonoBehaviour, ISceneConfiguration
    {
        [SerializeField]
        [Tooltip("Lists all assemblies whose property extensions will be used in the current scene.")]
        private List<string> extensionAssembliesWhitelist = new List<string>();

        [SerializeField]
        [Tooltip("Default resources prefab to use for the Confetti behavior.")]
        private string defaultConfettiPrefab;

        /// <inheritdoc/>
        public IEnumerable<string> ExtensionAssembliesWhitelist => extensionAssembliesWhitelist;

        /// <inheritdoc/>
        public string DefaultConfettiPrefab
        {
            get { return defaultConfettiPrefab; }
            set { defaultConfettiPrefab = value; }
        }

        /// <inheritdoc/>
        public bool IsAllowedInAssembly(Type extensionType, string assemblyName)
        {
            if (ExtensionAssembliesWhitelist.Contains(assemblyName) == false)
            {
                return false;
            }

            PropertyExtensionExclusionList blacklist = GetComponents<PropertyExtensionExclusionList>().FirstOrDefault(blacklist => blacklist.AssemblyFullName == assemblyName);

            if (blacklist == null)
            {
                return true;
            }
            else
            {
                return blacklist.DisallowedExtensionTypes.Any(disallowedType => disallowedType.FullName == extensionType.FullName) == false;
            }
        }

        /// <summary>
        /// Adds the specified assembly names to the extension whitelist.
        /// </summary>
        public void AddWhitelistAssemblies(IEnumerable<string> assemblyNames)
        {
            foreach (string assemblyName in assemblyNames)
            {
                if (extensionAssembliesWhitelist.Contains(assemblyName) == false)
                {
                    extensionAssembliesWhitelist.Add(assemblyName);
                }
            }
        }
    }
}
