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
            string oldURL = currentValue as string;
            T videoClip = Resources.Load<T>(oldURL);

            videoClip = EditorGUI.ObjectField(rect, label, videoClip, typeof(T), false) as T;

            string newURL = AssetDatabase.GetAssetOrScenePath(videoClip);

            if (string.IsNullOrEmpty(newURL) == false)
            {
                if (newURL.IndexOf("Resources") >= 0)
                {
                    newURL = newURL.Remove(0, newURL.IndexOf("Resources") + 10);
                }
                else
                {
                    Debug.LogError("The object is not in the path of a 'Resources' folder.");
                    newURL = "";
                }

                if (newURL.Contains("."))
                {
                    newURL = newURL.Remove(newURL.LastIndexOf("."));
                }
            }

            if (oldURL != newURL)
            {
                ChangeValue(() => newURL, () => oldURL, changeValueCallback);
            }

            return rect;
        }
    }
}
