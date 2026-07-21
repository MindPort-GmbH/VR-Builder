using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Handles configuration specific to this scene.
    /// </summary>
    public class SceneService : MonoBehaviour, ISceneService
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

        /// <inheritdoc/>
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

        public void SetConfiguration(object configuration)
        {
        }

        public void Initialize()
        {
        }
    }
}