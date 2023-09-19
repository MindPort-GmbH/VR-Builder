using System;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Editor.UI.Drawers
{
    /// <summary>
    /// Draws an object field and sets the asset path of the dragged object.
    /// </summary>
    public abstract class AssetPathDrawer<T> : AbstractDrawer where T : UnityEngine.Object
    {
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            string oldURL = currentValue as string;
            T videoClip = AssetDatabase.LoadAssetAtPath<T>(oldURL);

            videoClip = EditorGUI.ObjectField(rect, label, videoClip, typeof(T), false) as T;

            string newURL = AssetDatabase.GetAssetOrScenePath(videoClip);

            if (oldURL != newURL)
            {
                ChangeValue(() => newURL, () => oldURL, changeValueCallback);
            }

            return rect;
        }
    }
}
