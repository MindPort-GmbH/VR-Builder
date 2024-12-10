// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.UI
{
    /// <summary>
    /// Utility class to help with different unity color schemes (dark/light). Takes care about loading the given icon,
    /// will add _light or _dark to the given path. If only one icon is found it will be used for both skins. If no icon
    /// is found there will be an NullReferenceException thrown.
    ///
    /// DO NOT ADD FILE ENDINGS TO THE PATH!
    /// </summary>
    public class EditorIcon
    {
        private const string LightTextureFileEnding = "_light";
        private const string DarkTextureFileEnding = "_dark";

        private Texture2D iconLight;
        private Texture2D iconDark;

        private readonly string path;
        private bool isInitialized = false;

        /// <summary>
        /// Returns the texture of the icon, depending on the skin used.
        /// </summary>
        public Texture Texture
        {
            get
            {
                if (isInitialized == false)
                {
                    Initialize();
                }

                return EditorGUIUtility.isProSkin ? iconLight : iconDark;
            }
        }

        public EditorIcon(string path)
        {
            this.path = path;
        }

        private void Initialize()
        {
            isInitialized = true;
            iconLight = Resources.Load<Texture2D>(path + LightTextureFileEnding);
            iconDark = Resources.Load<Texture2D>(path + DarkTextureFileEnding);

            if (iconLight == null && iconDark == null)
            {
                string msg = string.Format("Texture with path: '{0}' couldn't be loaded. No {1} nor {2} version found!", path, LightTextureFileEnding, DarkTextureFileEnding);
                UnityEngine.Debug.LogError(msg);
                throw new NullReferenceException(msg);
            }

            if (iconLight == null)
            {
                if (EditorGUIUtility.isProSkin)
                {
                    UnityEngine.Debug.LogWarningFormat("No texture found for path: {0}", path + LightTextureFileEnding);
                }
                iconLight = iconDark;
            }
            else if (iconDark == null)
            {
                if (!EditorGUIUtility.isProSkin)
                {
                    UnityEngine.Debug.LogWarningFormat("No texture found for path: {0}", path + DarkTextureFileEnding);
                }
                iconDark = iconLight;
            }
        }
    }
}
