// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace VRBuilder.PackageManager.Editor
{
    /// <summary>
    /// Utility class for adding missing layers to the Unity's TagManager.
    /// </summary>
    internal static class LayerUtils
    {
        /// <summary>
        /// Adds given <paramref name="layer"/> to the Unity's TagManager.
        /// </summary>
        public static void AddLayer(string layer)
        {
            string[] layers = { layer };
            AddLayers(layers);
        }

        public static void AddLayerDependencies(IEnumerable<LayerDependency> layerDependencies)
        {
            Object[] foundAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");

            if (foundAsset.Length != 0 == false)
            {
                throw new FileLoadException("There was a problem trying to load ProjectSettings/TagManager.asset");
            }

            SerializedObject tagManager = new SerializedObject(foundAsset.First());
            SerializedProperty layersField = tagManager.FindProperty("layers");

            if (layersField == null || layersField.isArray == false)
            {
                throw new ArgumentException("Field layers is either null or not array.");
            }

            foreach (LayerDependency layerDependency in layerDependencies)
            {
                if (layerDependency.PreferredPosition < 0)
                {
                    AddLayer(layerDependency.Name);
                }
                // First 8 slots are reserved by Unity.
                else if (layerDependency.PreferredPosition < 8)
                {
                    throw new IndexOutOfRangeException($"Unable to create layer '{layerDependency.Name}' at position {layerDependency.PreferredPosition}. Layers 0-7 are reserved by Unity.");
                }
                else if (layerDependency.PreferredPosition < layersField.arraySize)
                {
                    SerializedProperty serializedProperty = layersField.GetArrayElementAtIndex(layerDependency.PreferredPosition);
                    string currentLayer = serializedProperty.stringValue;

                    if (currentLayer != layerDependency.Name)
                    {
                        if (string.IsNullOrEmpty(currentLayer) || ShowLayerExistsDialog(layerDependency, currentLayer))
                        {
                            serializedProperty.stringValue = layerDependency.Name;
                        }
                    }
                }
                else
                {
                    throw new IndexOutOfRangeException($"Unable to create layer '{layerDependency.Name}' at position {layerDependency.PreferredPosition}. Requested index is out of range.");
                }
            }

            tagManager.ApplyModifiedProperties();
        }

        private static bool ShowLayerExistsDialog(LayerDependency layerDependency, string currentLayer)
        {
            string title = "Layer position is not empty";
            string description = $"Attempted to create layer '{layerDependency.Name}' at position {layerDependency.PreferredPosition}. The position is not empty but occupied by layer '{currentLayer}'.";
            string yes = "Overwrite";
            string no = "Skip";
            return EditorUtility.DisplayDialog(title, description, yes, no);
        }

        /// <summary>
        /// Adds given <paramref name="layers"/> to the Unity's TagManager.
        /// </summary>
        /// <exception cref="FileLoadException">Exception thrown if the TagManager was not found.</exception>
        /// <exception cref="ArgumentException">Exception thrown if layers field is not found or is not an array.</exception>
        /// <remarks> The first 8 layers are reserved by Unity and will be ignored.</remarks>
        public static void AddLayers(IEnumerable<string> layers)
        {
            Object[] foundAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");

            if (foundAsset.Length != 0 == false)
            {
                throw new FileLoadException("There was a problem trying to load ProjectSettings/TagManager.asset");
            }

            SerializedObject tagManager = new SerializedObject(foundAsset.First());
            SerializedProperty layersField = tagManager.FindProperty("layers");
            Queue<string> newLayers = new Queue<string>(layers);

            if (layersField == null || layersField.isArray == false)
            {
                throw new ArgumentException("Field layers is either null or not array.");
            }

            // First 8 slots are reserved by Unity.
            for (int i = 8; i < layersField.arraySize; i++)
            {
                if (newLayers.Any())
                {
                    SerializedProperty serializedProperty = layersField.GetArrayElementAtIndex(i);
                    string stringValue = serializedProperty.stringValue;
                    string newLayer = newLayers.Peek();

                    if (stringValue == newLayer)
                    {
                        newLayers.Dequeue();
                        continue;
                    }

                    if (string.IsNullOrEmpty(stringValue))
                    {
                        serializedProperty.stringValue = newLayers.Dequeue();
                    }
                }
            }

            tagManager.ApplyModifiedProperties();
        }
    }
}
