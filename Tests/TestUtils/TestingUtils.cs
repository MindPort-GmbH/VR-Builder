// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Tests.Utils
{
    public static class TestingUtils
    {
        /// <summary>
        /// Returns value of field <paramref name="fieldName"/> of type <typeparamref name="T"></typeparamref>object <paramref name="o"/>
        /// </summary>
        public static T GetField<T>(object o, string fieldName)
        {
            FieldInfo[] fields = o.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToArray();
            return (T)fields.First(f => f.Name == fieldName).GetValue(o);
        }

        /// <summary>
        /// Returns value of field <paramref name="fieldName"/> of type <typeparamref name="TResult"/> in object <paramref name="o"/> of type <typeparamref name="TObject"/>, including private declarations.
        /// </summary>
        public static TResult GetFieldInExactType<TResult, TObject>(TObject o, string fieldName)
        {
            FieldInfo[] fields = typeof(TObject).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToArray();
            return (TResult)fields.First(f => f.Name == fieldName).GetValue(o);
        }

        /// <summary>
        /// Returns value of property <paramref name="propertyName"/> of type <typeparamref name="T"></typeparamref>object <paramref name="o"/>
        /// </summary>
        public static T GetPropertyValue<T>(object o, string propertyName)
        {
            return (T)o.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First(p => p.Name == propertyName).GetValue(o, null);
        }

        /// <summary>
        /// Destroys game objects to which <paramref name="obj"/> is attached.
        /// </summary>
        public static void DestroySceneObject(ISceneObject obj)
        {
            Object.DestroyImmediate(obj.GameObject);
        }

        /// <summary>
        /// Destroys game objects to which <paramref name="property"/> is attached.
        /// </summary>
        public static void DestroySceneObject(ISceneObjectProperty property)
        {
            DestroySceneObject(property.SceneObject);
        }

        /// <summary>
        /// Creates game object and attaches <see cref="ProcessSceneObject"/> component with the name <paramref name="name"/> to it.
        /// </summary>
        public static ProcessSceneObject CreateSceneObject(string name, GameObject gameObject = null)
        {
            if (gameObject == null)
            {
                gameObject = new GameObject(name);
            }

            ProcessSceneObject processSceneObject = gameObject.AddComponent<ProcessSceneObject>();

            return processSceneObject;
        }

        /// <summary>
        /// As <see cref="CreateSceneObject"/> but also attaches component of type <typeparamref name="T1"/>.
        /// </summary>
        public static ProcessSceneObject CreateSceneObject<T1>(string name, GameObject gameObject = null) where T1 : Component
        {
            if (gameObject == null)
            {
                gameObject = new GameObject(name);
            }

            gameObject.AddComponent<T1>();
            return CreateSceneObject(name, gameObject);
        }

        /// <summary>
        /// As <see cref="CreateSceneObject"/> but also attaches components of types <typeparamref name="T1"/>, <typeparamref name="T2"/>.
        /// </summary>
        public static ProcessSceneObject CreateSceneObject<T1, T2>(string name, GameObject gameObject = null) where T1 : Component where T2 : Component
        {
            if (gameObject == null)
            {
                gameObject = new GameObject(name);
            }

            gameObject.AddComponent<T2>();
            return CreateSceneObject<T1>(name, gameObject);
        }

        /// <summary>
        /// As <see cref="CreateSceneObject"/> but also attaches components of types <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>.
        /// </summary>
        public static ProcessSceneObject CreateSceneObject<T1, T2, T3>(string name, GameObject gameObject = null) where T1 : Component where T2 : Component where T3 : Component
        {
            if (gameObject == null)
            {
                gameObject = new GameObject(name);
            }

            gameObject.AddComponent<T3>();
            return CreateSceneObject<T1, T2>(name, gameObject);
        }

        /// <summary>
        /// As <see cref="CreateSceneObject"/> but also attaches components of types <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        public static ProcessSceneObject CreateSceneObject<T1, T2, T3, T4>(string name, GameObject gameObject = null) where T1 : Component where T2 : Component where T3 : Component where T4 : Component
        {
            if (gameObject == null)
            {
                gameObject = new GameObject(name);
            }

            gameObject.AddComponent<T4>();
            return CreateSceneObject<T1, T2, T3>(name, gameObject);
        }

        /// <summary>
        /// Creates a process scene object prefab.
        /// </summary>
        /// <param name="prefabName">The name of the prefab.</param>
        /// <param name="prefabPath">The path of the prefab.</param>
        /// <returns>The created GameObject prefab.</returns>
        public static GameObject CreateProcessSceneObjectPrefab(string prefabName, string prefabPath)
        {
            ProcessSceneObject processSceneObject = CreateSceneObject(prefabName);
            return CreatePrefab(processSceneObject.gameObject, prefabPath);
        }

        /// <summary>
        /// Creates a folder recursively at the specified path if it doesn't already exist.
        /// </summary>
        /// <param name="path">The path of the folder to create.</param>
        /// <returns>The GUID of the created folder if the folder was created successfully other wise an empty string.</returns>
        /// <remarks>
        /// Make sure to delete the folder after the test is done.
        /// </remarks>
        public static string CreateFolderRecursively(string path)
        {
            if (AssetDatabase.IsValidFolder(path) == false)
            {
                string parentPath = Path.GetDirectoryName(path);
                if (!AssetDatabase.IsValidFolder(parentPath))
                {
                    CreateFolderRecursively(parentPath);
                }

                return AssetDatabase.CreateFolder(parentPath, Path.GetFileName(path));
            }
            return "";
        }

        /// <summary>
        /// Deletes the content and folder at the specified prefab path.
        /// </summary>
        /// <param name="prefabPath">The path of the prefab folder to delete.</param>
        /// <returns><c>true</c> if the content and folder were deleted; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Be careful with this method, to only delete data you created for testing.
        /// </remarks>
        public static bool DeleteContentAndFolder(string prefabPath)
        {
            if (AssetDatabase.IsValidFolder(prefabPath))
            {
                string[] assets = AssetDatabase.FindAssets("", new[] { prefabPath });

                foreach (string assetGUID in assets)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                    AssetDatabase.DeleteAsset(assetPath);
                }

                return AssetDatabase.DeleteAsset(prefabPath);
            }
            return false;
        }

        /// <summary>
        /// Creates a prefab from the specified original GameObject and saves it at the specified prefab path.
        /// </summary>
        /// <param name="originalObject">The original GameObject to create the prefab from.</param>
        /// <param name="prefabPath">The path where the prefab will be saved.</param>
        /// <returns>The created prefab as a GameObject.</returns>
        public static GameObject CreatePrefab(GameObject originalObject, string prefabPath)
        {
            string prefabName = originalObject.name;
            PrefabUtility.SaveAsPrefabAsset(originalObject, $"{prefabPath}/{prefabName}.prefab");
            GameObject.DestroyImmediate(originalObject);
            return AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}/{prefabName}.prefab");
        }

        /// <summary>
        /// Generates a unique name.
        /// </summary>
        /// <returns>A string representing the unique name.</returns>
        public static string GetUniqueName()
        {
            return $"UniqueName-{System.Guid.NewGuid()}";
        }

        /// <summary>
        /// Deletes a prefab from the specified path.
        /// </summary>
        /// <param name="prefabName">The name of the prefab to delete.</param>
        /// <param name="prefabPath">The path where the prefab is located.</param>
        /// <returns><c>true</c> if the prefab was deleted; otherwise, <c>false</c>.</returns>
        public static bool DeletePrefab(string prefabName, string prefabPath)
        {
            return AssetDatabase.DeleteAsset($"{prefabPath}/{prefabName}.prefab");
        }
    }
}
