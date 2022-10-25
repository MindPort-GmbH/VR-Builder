using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Editor.Settings;

namespace VRBuilder.Editor.UI.Drawers
{
    public class SceneObjectTagDrawer : AbstractDrawer
    {
        private const string noComponentSelected = "<none>";

        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            rect.height = EditorDrawingHelper.SingleLineHeight;            

            Guid oldValue = (Guid)currentValue;

            SceneObjectTags.Tag[] tags = SceneObjectTags.Instance.Tags.ToArray();
            List<string> labels = tags.Select(tag => tag.Label).ToList();

            EditorGUI.BeginDisabledGroup(tags.Length == 0);

            SceneObjectTags.Tag currentTag = tags.FirstOrDefault(tag => tag.Guid == oldValue);

            int selectedTagIndex = Array.IndexOf(tags, currentTag);
            bool isTagInvalid = false;

            if(selectedTagIndex == -1)
            {
                selectedTagIndex = 0;
                labels.Insert(0, noComponentSelected);
                isTagInvalid = true;
            }

            selectedTagIndex = EditorGUI.Popup(rect, selectedTagIndex, labels.ToArray());
            EditorGUI.EndDisabledGroup();

            if(isTagInvalid && selectedTagIndex == 0)
            {
                return rect;
            }
            else if (isTagInvalid)
            {
                selectedTagIndex--;
            }

            Guid newValue = tags[selectedTagIndex].Guid;

            if (oldValue != newValue)
            {
                ChangeValue(() => newValue, () => oldValue, changeValueCallback);
            }

            return rect;
        }
    }
}
