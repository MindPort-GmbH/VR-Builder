using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.UI.Drawers
{
    [DefaultProcessDrawer(typeof(UniqueNameReference))]
    public class SceneReferenceDrawer : AbstractDrawer
    {
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            UniqueNameReference nameReference = (UniqueNameReference)currentValue;

            Rect nextPosition = new Rect(rect.x, rect.y, rect.width, rect.height);
            nextPosition = DrawerLocator.GetDrawerForValue(nameReference.SceneReferenceType, typeof(SceneReferenceType)).Draw(nextPosition, nameReference.SceneReferenceType, (value) => nameReference.SceneReferenceType = (SceneReferenceType)value, label);
            float height = EditorDrawingHelper.SingleLineHeight + EditorDrawingHelper.VerticalSpacing;

            nextPosition.y = rect.y + height;


            switch (nameReference.SceneReferenceType)
            {
                case SceneReferenceType.Object:
                    nextPosition = new UniqueNameReferenceDrawer().Draw(nextPosition, currentValue, changeValueCallback, " ");
                    height += nextPosition.height;
                    nextPosition.y = rect.y + height;
                    CheckForMultipleObjects(nameReference, ref rect, ref nextPosition, ref height);
                    break;
                case SceneReferenceType.Category:
                    nextPosition = new SceneObjectTagDrawer().Draw(nextPosition, GetTagFromUniqueNameReference(nameReference), (value) => nameReference.Guid = ((SceneObjectTagBase)value).Guid, " ");
                    height += nextPosition.height;
                    nextPosition.y = rect.y + height;
                    break;
            }


            rect.height = height;
            return rect;
        }

        private void CheckForMultipleObjects(UniqueNameReference nameReference, ref Rect originalRect, ref Rect nextPosition, ref float height)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }

            if (RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(nameReference.Guid).Count() > 1)
            {
                nextPosition.y += EditorDrawingHelper.VerticalSpacing;
                EditorGUI.HelpBox(nextPosition, "Multiple valid objects found in the scene.", MessageType.Warning);
                height += nextPosition.height + EditorDrawingHelper.VerticalSpacing;
                nextPosition.y = originalRect.y + height;
            }
        }

        private SceneObjectTagBase GetTagFromUniqueNameReference(UniqueNameReference reference)
        {
            Type tagType = typeof(SceneObjectTag<>);
            Type propertyType = reference.GetReferenceType();
            Type constructedTag = tagType.MakeGenericType(propertyType);
            SceneObjectTagBase tag = Activator.CreateInstance(constructedTag) as SceneObjectTagBase;
            tag.Guid = reference.Guid;
            return tag;
        }
    }
}
