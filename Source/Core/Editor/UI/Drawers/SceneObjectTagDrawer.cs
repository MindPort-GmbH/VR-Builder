using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Editor.Settings;

namespace VRBuilder.Editor.UI.Drawers
{
    [DefaultProcessDrawer(typeof(SceneObjectTag))]
    public class SceneObjectTagDrawer : AbstractDrawer
    {
        private const string noComponentSelected = "<none>";

        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            rect.height = EditorDrawingHelper.SingleLineHeight;

            SceneObjectTag oldTag = (SceneObjectTag)currentValue;
            Guid oldGuid = oldTag.Guid;

            SceneObjectTags.Tag[] tags = SceneObjectTags.Instance.Tags.ToArray();
            List<string> labels = tags.Select(tag => tag.Label).ToList();

            EditorGUI.BeginDisabledGroup(tags.Length == 0);

            SceneObjectTags.Tag currentTag = tags.FirstOrDefault(tag => tag.Guid == oldGuid);

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

            Guid newGuid = tags[selectedTagIndex].Guid;

            if (oldGuid != newGuid)
            {
                SceneObjectTag newTag = new SceneObjectTag(newGuid, oldTag.PropertyType);
                ChangeValue(() => newTag, () => oldTag, changeValueCallback);
            }

            return rect;
        }
    }
}
