using System;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Editor.UI.Drawers
{
    /// <summary>
    /// Draws an object field and sets the Resources path of the dragged object.
    /// </summary>
    public abstract class ResourcePathDrawer<T> : AbstractDrawer where T : UnityEngine.Object
    {
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            string oldPath = currentValue as string;
            T videoClip = Resources.Load<T>(oldPath);

            videoClip = EditorGUI.ObjectField(rect, label, videoClip, typeof(T), false) as T;

            string newPath = AssetDatabase.GetAssetOrScenePath(videoClip);

            if (string.IsNullOrEmpty(newPath) == false)
            {
                if (newPath.IndexOf("Resources") >= 0)
                {
                    newPath = newPath.Remove(0, newPath.IndexOf("Resources") + 10);
                }
                else
                {
                    Debug.LogError("The object is not in the path of a 'Resources' folder.");
                    newPath = "";
                }

                if (newPath.Contains("."))
                {
                    newPath = newPath.Remove(newPath.LastIndexOf("."));
                }
            }

            if (oldPath != newPath)
            {
                ChangeValue(() => newPath, () => oldPath, changeValueCallback);
            }

            return rect;
        }
    }
}
