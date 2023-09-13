using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRBuilder.Core.Configuration
{
    public class PropertyExtensionExclusionList : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Full name of the assembly we want to exclude the types from.")]
        private string assemblyFullName;

        [SerializeField]
        [Tooltip("List of excluded extension type names, including namespaces.")]
        private List<string> disallowedExtensionTypeNames;

        /// <summary>
        /// Full name of the assembly we want to exclude the types from.
        /// </summary>
        public string AssemblyFullName => assemblyFullName;

        /// <summary>
        /// List of excluded extension types.
        /// </summary>
        public IEnumerable<Type> DisallowedExtensionTypes
        {
            get
            {
                IEnumerable<string> assemblyQualifiedNames = disallowedExtensionTypeNames.Select(typeName => $"{typeName}, {assemblyFullName}");
                List<Type> excludedTypes = new List<Type>();

                foreach (string typeName in assemblyQualifiedNames)
                {
                    Type excludedType = Type.GetType(typeName);

                    if (excludedType == null)
                    {
                        Debug.LogWarning($"Property extension exclusion list for assembly '{assemblyFullName}' contains invalid extension type: '{typeName}'.");
                    }
                    else
                    {
                        excludedTypes.Add(excludedType);
                    }
                }

                return excludedTypes;
            }
        }
    }
}
