using System;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Draws an object field and sets the Resources path of the dragged object.
    /// </summary>
    public abstract class ResourcePathDrawer<T> : AbstractDrawer where T : UnityEngine.Object
    {
        /// <summary>
        /// Validates the resource just dragged in the drawer.
        /// </summary>
        public abstract void ValidateResource(T resource);

        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            string oldPath = currentValue as string;
            T oldResource = Resources.Load<T>(oldPath);

            T resource = EditorGUI.ObjectField(rect, label, oldResource, typeof(T), false) as T;

            string newPath = AssetDatabase.GetAssetOrScenePath(resource);

            if (string.IsNullOrEmpty(newPath) == false)
            {
                if (newPath.IndexOf("Resources") >= 0)
                {
                    newPath = newPath.Remove(0, newPath.IndexOf("Resources") + 10);
                }
                else
                {
                    UnityEngine.Debug.LogError("The object is not in the path of a 'Resources' folder.");
                    newPath = "";
                }

                if (newPath.Contains('.'))
                {
                    newPath = newPath.Remove(newPath.LastIndexOf("."));
                }
            }

            if (resource != null && resource != oldResource && string.IsNullOrEmpty(newPath) == false)
            {
                ValidateResource(resource);
            }

            if (oldPath != newPath)
            {
                ChangeValue(() => newPath, () => oldPath, changeValueCallback);
            }

            return rect;
        }
    }
}
