// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using VRBuilder.Core.Editor.ProcessAssets;
using VRBuilder.Core.Serialization;

namespace VRBuilder.Core.Editor.Menu
{
    internal static class ImportProcessMenuEntry
    {
        /// <summary>
        /// Allows importing processes.
        /// </summary>
        [MenuItem("Tools/VR Builder/Developer/Import Process...", false, 1000)]
        private static void ImportProcess()
        {
            string path = EditorUtility.OpenFilePanel("Select your process", ".", String.Empty);

            if (string.IsNullOrEmpty(path) || Directory.Exists(path))
            {
                return;
            }

            string format = Path.GetExtension(path).Replace(".", "");
            List<IProcessSerializer> result = GetFittingSerializer(format);

            if (result.Count == 0)
            {
                UnityEngine.Debug.LogError("Tried to import, but no Serializer found.");
                return;
            }

            if (result.Count == 1)
            {
                ProcessAssetManager.Import(path, result.First());
            }
            else
            {
                ChooseSerializerPopup.Show(result, (serializer) =>
                {
                    ProcessAssetManager.Import(path, serializer);
                });
            }
        }

        private static List<IProcessSerializer> GetFittingSerializer(string format)
        {
            return TypeCache.GetTypesDerivedFrom<IProcessSerializer>()
                .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
                .Select(type => (IProcessSerializer)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<object>(), null))
                .Where(s => s.FileFormat.Equals(format))
                .ToList();
        }
    }
}
