using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Editor.Settings;

namespace VRBuilder.Editor.UI.Drawers
{
    public class SceneObjectTagDrawer : AbstractDrawer
    {        
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            rect.height = EditorDrawingHelper.SingleLineHeight;

            Guid oldValue = (Guid)currentValue;

            SceneObjectTags.Tag[] tags = SceneObjectTags.Instance.Tags.ToArray();

            EditorGUI.BeginDisabledGroup(tags.Length == 0);

            SceneObjectTags.Tag currentTag = tags.FirstOrDefault(tag => tag.Guid == oldValue);

            int selectedTagIndex = Array.IndexOf(tags, currentTag);

            if(selectedTagIndex == -1)
            {
                selectedTagIndex = 0;
            }

            selectedTagIndex = EditorGUI.Popup(rect, selectedTagIndex, tags.Select(tag => tag.Label).ToArray());
            EditorGUI.EndDisabledGroup();

            Guid newValue = tags[selectedTagIndex].Guid;

            if (oldValue != newValue)
            {
                ChangeValue(() => newValue, () => oldValue, changeValueCallback);
            }

            return rect;
        }
    }
}
