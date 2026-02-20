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
        private static string cachedEntityClipboardContent;
        private static IEntity cachedEntityClipboardValue;
        private static bool hasCachedEntityClipboardValue;
        private static bool isEntityClipboardCacheInitialized;

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
        /// Retrieves a cached preview entity from the clipboard if available.
        /// This avoids repeated deserialization during IMGUI draw enable/disable checks.
        /// </summary>
        public static bool TryPeekEntity(out IEntity entity)
        {
            EnsureEntityClipboardCache();
            entity = cachedEntityClipboardValue;
            return hasCachedEntityClipboardValue;
        }

        /// <summary>
        /// Serializes a <see cref="IEntity"/> to the clipboard.
        /// </summary>
        /// <param name="entity">Entity to serialize.</param>
        public static void CopyEntity(IEntity entity)
        {
            byte[] serialized = EditorConfigurator.Instance.Serializer.EntityToByteArray(entity);
            EditorGUIUtility.systemCopyBuffer = Encoding.UTF8.GetString(serialized);
            InvalidateEntityClipboardCache();
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
            EnsureEntityClipboardCache();
            return hasCachedEntityClipboardValue;
        }

        /// <summary>
        /// Checks whether the current clipboard entity can be cast to <typeparamref name="TEntity"/>.
        /// </summary>
        public static bool IsEntityInClipboard<TEntity>() where TEntity : class, IEntity
        {
            EnsureEntityClipboardCache();
            return cachedEntityClipboardValue is TEntity;
        }

        private static void EnsureEntityClipboardCache()
        {
            string currentClipboardContent = EditorGUIUtility.systemCopyBuffer;
            if (isEntityClipboardCacheInitialized && currentClipboardContent == cachedEntityClipboardContent)
            {
                return;
            }

            cachedEntityClipboardContent = currentClipboardContent;
            isEntityClipboardCacheInitialized = true;

            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(currentClipboardContent);
                cachedEntityClipboardValue = EditorConfigurator.Instance.Serializer.EntityFromByteArray(bytes);
                hasCachedEntityClipboardValue = cachedEntityClipboardValue != null;
            }
            catch
            {
                cachedEntityClipboardValue = null;
                hasCachedEntityClipboardValue = false;
            }
        }

        private static void InvalidateEntityClipboardCache()
        {
            isEntityClipboardCacheInitialized = false;
        }
    }
}
