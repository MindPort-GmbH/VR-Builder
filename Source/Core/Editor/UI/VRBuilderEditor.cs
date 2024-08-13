using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Editor.UI
{
    public abstract class VRBuilderEditor : UnityEditor.Editor
    {
        protected abstract Texture2D Icon { get; }
        protected abstract UnityEngine.Object[] Targets { get; }

        protected void SetComponentIcon()
        {
            if (Icon != null)
            {
                SetUnityObjectIcon(target, Icon);
            }
        }

        private void SetUnityObjectIcon(UnityEngine.Object unityObject, Texture2D icon)
        {
            if (unityObject != null)
            {
                Type editorGUIUtilityType = typeof(EditorGUIUtility);
                BindingFlags bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
                object[] args = new object[] { unityObject, icon };
                editorGUIUtilityType.InvokeMember("SetIconForObject", bindingFlags, null, null, args);
            }
        }

        protected void DrawCustomHeaderIcon()
        {
            // Use reflection to access the internal method that draws the script icon
            var editorAssembly = typeof(UnityEditor.Editor).Assembly;
            var scriptAttributeUtilityType = editorAssembly.GetType("UnityEditor.ScriptAttributeUtility");
            var getIconForObject = scriptAttributeUtilityType.GetMethod("GetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);

            // Replace this with your icon texture
            Texture2D customIcon = Icon;

            if (customIcon != null)
            {
                // Use internal method to override the icon (this is a workaround)
                GUIContent titleContent = new GUIContent(serializedObject.targetObject.name, customIcon);
                var titleContentProperty = typeof(UnityEditor.Editor).GetProperty("titleContent", BindingFlags.Instance | BindingFlags.Public);
                titleContentProperty.SetValue(this, titleContent, null);
            }
        }
    }
}
