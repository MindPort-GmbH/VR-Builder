using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace VRBuilder.Editor.UI.Drawers
{
    public class AssetPathDrawer : AbstractDrawer
    {
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            string oldURL = currentValue as string;
            VideoClip videoClip = AssetDatabase.LoadAssetAtPath<VideoClip>(oldURL);

            videoClip = EditorGUI.ObjectField(rect, label, videoClip, typeof(VideoClip), false) as VideoClip;

            string newURL = AssetDatabase.GetAssetOrScenePath(videoClip);

            if (oldURL != newURL)
            {
                ChangeValue(() => newURL, () => oldURL, changeValueCallback);
            }

            return rect;

        }
    }
}
