// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System.Text;
using UnityEditor;
using VRBuilder.Core;
using VRBuilder.Core.Editor.Configuration;

namespace VRBuilder.Core.Editor.Utils
{
    /// <summary>
    /// A utility class that converts a step from/to UTF-8 string using the serializer from the current <see cref="IEditorConfiguration"/>
    /// and then copies or pastes it from the system's copy buffer.
    /// </summary>
    internal static class SystemClipboard
    {
        /// <summary>
        /// Tries to deserialize step from the system's copy buffer. Does not handle exceptions.
        /// </summary>
        /// <returns>A new instance of step.</returns>
        public static IStep PasteStep()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(EditorGUIUtility.systemCopyBuffer);
            return EditorConfigurator.Instance.Serializer.StepFromByteArray(bytes);
        }

        /// <summary>
        /// Serializes the <paramref name="step"/> to a byte array, converts this array into UTF-8 string, and saves it to the system's copy buffer.
        /// </summary>
        /// <param name="step">A step to serialize.</param>
        public static void CopyStep(IStep step)
        {
            byte[] serialized = EditorConfigurator.Instance.Serializer.StepToByteArray(step);
            EditorGUIUtility.systemCopyBuffer = Encoding.UTF8.GetString(serialized);
        }

        /// <summary>
        /// Retrives a <see cref="IEntity"/> from the clipboard.
        /// </summary>        
        public static IEntity PasteEntity()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(EditorGUIUtility.systemCopyBuffer);
            return EditorConfigurator.Instance.Serializer.EntityFromByteArray(bytes);
        }

        /// <summary>
        /// Serializes a <see cref="IEntity"/> to the clipboard.
        /// </summary>
        /// <param name="entity">Entity to serialize.</param>
        public static void CopyEntity(IEntity entity)
        {
            byte[] serialized = EditorConfigurator.Instance.Serializer.EntityToByteArray(entity);
            EditorGUIUtility.systemCopyBuffer = Encoding.UTF8.GetString(serialized);
        }

        /// <summary>
        /// Checks if there is a valid serialized step in the system's copy buffer.
        /// </summary>
        public static bool IsStepInClipboard()
        {
            try
            {
                return (PasteStep() != null);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if there is a valid serialized entity in the system's copy buffer.
        /// </summary>
        public static bool IsEntityInClipboard()
        {
            try
            {
                return PasteEntity() != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
