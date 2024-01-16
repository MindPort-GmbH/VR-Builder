using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.UI.Drawers
{
    [DefaultProcessDrawer(typeof(SceneObjectTagBase))]
    public class SceneReferenceDrawer : AbstractDrawer
    {
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            SceneObjectTagBase baseIdentifier = (SceneObjectTagBase)currentValue;

            Rect nextPosition = new Rect(rect.x, rect.y, rect.width, rect.height);
            nextPosition = DrawerLocator.GetDrawerForValue(baseIdentifier.InspectorType, typeof(InspectorType)).Draw(nextPosition, baseIdentifier.InspectorType, (value) => baseIdentifier.InspectorType = (InspectorType)value, label);
            float height = EditorDrawingHelper.SingleLineHeight + EditorDrawingHelper.VerticalSpacing;

            nextPosition.y = rect.y + height;


            switch (baseIdentifier.InspectorType)
            {
                case InspectorType.Object:
                    nextPosition = new SingleTagReferenceDrawer().Draw(nextPosition, currentValue, changeValueCallback, " ");
                    height += nextPosition.height;
                    nextPosition.y = rect.y + height;
                    CheckForMultipleObjects(baseIdentifier, ref rect, ref nextPosition, ref height);
                    break;
                case InspectorType.Category:
                    nextPosition = new SceneObjectTagDrawer().Draw(nextPosition, currentValue, changeValueCallback, " ");
                    height += nextPosition.height;
                    nextPosition.y = rect.y + height;
                    CheckForMultipleObjects(baseIdentifier, ref rect, ref nextPosition, ref height);
                    break;
            }


            rect.height = height;
            return rect;
        }

        private void CheckForMultipleObjects(SceneObjectTagBase nameReference, ref Rect originalRect, ref Rect nextPosition, ref float height)
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

        private UniqueNameReference GetSceneObjectReferenceFromTag(SceneObjectTagBase identifier)
        {
            bool isSceneObject = identifier.GetReferenceType().IsAssignableFrom(typeof(ISceneObject));
            Type referenceType = isSceneObject ? typeof(SceneObjectReference) : typeof(ScenePropertyReference<>);
            Type propertyType = identifier.GetReferenceType();
            Type constructedType = isSceneObject ? referenceType : referenceType.MakeGenericType(propertyType);
            UniqueNameReference reference = Activator.CreateInstance(constructedType) as UniqueNameReference;
            reference.Guid = identifier.Guid;
            return reference;
        }
    }
}
