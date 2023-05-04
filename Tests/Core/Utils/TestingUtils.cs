// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System.Linq;
using System.Reflection;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Properties;
using UnityEngine;

namespace VRBuilder.Tests.Utils
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
    }
}
